using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using CVScreeningService.Services.Screening;
using CVScreeningWeb.Helpers;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure.Implementation;
using Kendo.Mvc.UI;
using Nalysa.Common.Log;

namespace CVScreeningWeb.Controllers
{
    [Authorize]
    [Filters.HandleError]
    public class AtomicCheckEditorImageBrowserController : EditorImageBrowserController
    {

        private readonly string ContentFolderRoot;

        private const int ThumbnailHeight = 80;
        private const int ThumbnailWidth = 80;

        private readonly DirectoryBrowser _directoryBrowser;
        private readonly ThumbnailCreator _thumbnailCreator;
        private readonly DirectoryPermission _permission;
        private readonly VirtualPathProviderWrapper _pathProvider;

        private readonly IScreeningService _screeningService;



        /// <summary>
        /// Controller constructor
        /// </summary>
        public AtomicCheckEditorImageBrowserController(IScreeningService screeningService)
        {
            _directoryBrowser = new DirectoryBrowser();       
            _thumbnailCreator = new ThumbnailCreator(new FitImageResizer());
            _permission = new DirectoryPermission();
            _pathProvider = new VirtualPathProviderWrapper();
            _screeningService = screeningService;
            ContentFolderRoot = FileHelper.GetFileFolder();

        }

        /// <summary>
        /// Gets the base path from which content will be served.
        /// </summary>
        public override string ContentPath
        {
            get
            {
                return ContentFolderRoot;
            }
        }


        /// <summary>
        /// Is authorized to read
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override bool AuthorizeRead(string path)
        {
            return CanAccess(path);
        }


        /// <summary>
        /// Is able to access to the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected override bool CanAccess(string path)
        {
            LogManager.Instance.Info(
                string.Format("Function: {0}. CanAccess method " +
                              "ContentPath: {1}, " +
                              "_pathProvider.ToAbsolute(ContentPath): {2}, " +
                              "path: {3}",
                    MethodBase.GetCurrentMethod().Name, ContentPath, _pathProvider.ToAbsolute(ContentPath), path));
            return _permission.CanAccess(_pathProvider.ToAbsolute(ContentPath), path);
        }


        private string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return _pathProvider.ToAbsolute(ContentPath);
            }
            return _pathProvider.CombinePaths(_pathProvider.ToAbsolute(ContentPath), path);
        }


        public JsonResult ImageRead(string path, int id)
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            var atomicCheckImagePath = FileHelper.GetAtomicCheckPictureReportVirtualPath(
                atomicCheckDTO.Screening, atomicCheckDTO);
            
            path = string.IsNullOrEmpty(path) 
                ? _pathProvider.ToAbsolute(atomicCheckImagePath) 
                : _pathProvider.CombinePaths(_pathProvider.ToAbsolute(atomicCheckImagePath), path);

            if (AuthorizeRead(path))
            {
                try
                {
                    _directoryBrowser.Server = Server;

                    var result = _directoryBrowser.GetFiles(path, Filter)
                        .Concat(_directoryBrowser.GetDirectories(path));

                    return Json(result);
                }
                catch (DirectoryNotFoundException)
                {
                    throw new HttpException(404, "File Not Found");
                }
            }
            LogManager.Instance.Error(
                string.Format("Function: {0}. Not authorized to read directory. " +
                              "AtomicCheckId: {1}, " +
                              "AtomicCheckVirtualPath: {2}, " +
                              "Path: {3}",
                    MethodBase.GetCurrentMethod().Name, id, atomicCheckImagePath, path));
            throw new HttpException(403, "Forbidden");
        }

        public override bool AuthorizeThumbnail(string path)
        {
            return CanAccess(path);
        }

        /// <summary>
        /// Serves an image's thumbnail by given path.
        /// </summary>
        /// <param name="path">The path to the image.</param>
        /// <returns>Thumbnail of an image.</returns>
        /// <exception cref="HttpException">Throws 403 Forbidden if the <paramref name="path"/> is outside of the valid paths.</exception>
        /// <exception cref="HttpException">Throws 404 File Not Found if the <paramref name="path"/> refers to a non existant image.</exception>
        [OutputCache(Duration = 3600, VaryByParam = "path")]
        public ActionResult ImageThumbnail(string path, int id)
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            var atomicCheckImagePath = FileHelper.GetAtomicCheckPictureReportVirtualPath(
                atomicCheckDTO.Screening, atomicCheckDTO);

            path = string.IsNullOrEmpty(path)
                ? _pathProvider.ToAbsolute(atomicCheckImagePath)
                : _pathProvider.CombinePaths(_pathProvider.ToAbsolute(atomicCheckImagePath), path);

            if (AuthorizeThumbnail(path))
            {
                LogManager.Instance.Info("Path: " + path);
                var physicalPath = Server.MapPath(path);
                LogManager.Instance.Info("Physical path: " + physicalPath);

                if (System.IO.File.Exists(physicalPath))
                {
                    Response.AddFileDependency(physicalPath);

                    return CreateThumbnail(physicalPath);
                }
                else
                {
                    throw new HttpException(404, "File Not Found");
                }
            }
            else
            {
                throw new HttpException(403, "Forbidden");
            }
        }

        private FileContentResult CreateThumbnail(string physicalPath)
        {
            using (var fileStream = System.IO.File.OpenRead(physicalPath))
            {
                var desiredSize = new ImageSize
                {
                    Width = ThumbnailWidth,
                    Height = ThumbnailHeight
                };

                const string contentType = "image/png";

                return File(_thumbnailCreator.Create(fileStream, desiredSize, contentType), contentType);
            }
        }

        /// <summary>
        /// Deletes a entry.
        /// </summary>
        /// <param name="path">The path to the entry.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>An empty <see cref="ContentResult"/>.</returns>
        /// <exception cref="HttpException">Forbidden</exception>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImageDestroy(string path, ImageBrowserEntry entry, int id)
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            var atomicCheckImagePath = FileHelper.GetAtomicCheckPictureReportVirtualPath(
                atomicCheckDTO.Screening, atomicCheckDTO);

            path = string.IsNullOrEmpty(path)
                ? _pathProvider.ToAbsolute(atomicCheckImagePath)
                : _pathProvider.CombinePaths(_pathProvider.ToAbsolute(atomicCheckImagePath), path);

            if (entry != null)
            {
                path = _pathProvider.CombinePaths(path, entry.Name);
                if (entry.EntryType == ImageBrowserEntryType.File)
                {
                    DeleteFile(path);
                }
                else
                {
                    DeleteDirectory(path);
                }

                return Json(new object[0]);
            }
            throw new HttpException(404, "File Not Found");
        }

        /// <summary>
        /// Determines if a file can be deleted.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>true if file can be deleted, otherwise false.</returns>
        public override bool AuthorizeDeleteFile(string path)
        {
            return CanAccess(path);
        }

        /// <summary>
        /// Determines if a folder can be deleted.
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        /// <returns>true if folder can be deleted, otherwise false.</returns>
        public virtual bool AuthorizeDeleteDirectory(string path)
        {
            return CanAccess(path);
        }

        protected override void DeleteFile(string path)
        {
            if (!AuthorizeDeleteFile(path))
            {
                throw new HttpException(403, "Forbidden");
            }

            var physicalPath = Server.MapPath(path);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        protected override void DeleteDirectory(string path)
        {
            if (!AuthorizeDeleteDirectory(path))
            {
                throw new HttpException(403, "Forbidden");
            }

            var physicalPath = Server.MapPath(path);

            if (Directory.Exists(physicalPath))
            {
                Directory.Delete(physicalPath, true);
            }
        }

        /// <summary>
        /// Determines if a folder can be created. 
        /// </summary>
        /// <param name="path">The path to the parent folder in which the folder should be created.</param>
        /// <param name="name">Name of the folder.</param>
        /// <returns>true if folder can be created, otherwise false.</returns>
        public override bool AuthorizeCreateDirectory(string path, string name)
        {
            return CanAccess(path);
        }

        /// <summary>
        /// Creates a folder with a given entry.
        /// </summary>
        /// <param name="path">The path to the parent folder in which the folder should be created.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>An empty <see cref="ContentResult"/>.</returns>
        /// <exception cref="HttpException">Forbidden</exception>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImageCreate(string path, ImageBrowserEntry entry, int id)
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            var atomicCheckImagePath = FileHelper.GetAtomicCheckPictureReportVirtualPath(
                atomicCheckDTO.Screening, atomicCheckDTO);

            path = string.IsNullOrEmpty(path)
                ? _pathProvider.ToAbsolute(atomicCheckImagePath)
                : _pathProvider.CombinePaths(_pathProvider.ToAbsolute(atomicCheckImagePath), path); 
            
            var name = entry.Name;

            if (name.HasValue() && AuthorizeCreateDirectory(path, name))
            {
                var physicalPath = Path.Combine(Server.MapPath(path), name);

                if (!Directory.Exists(physicalPath))
                {
                    Directory.CreateDirectory(physicalPath);
                }

                return Json(entry);
            }

            throw new HttpException(403, "Forbidden");
        }

        /// <summary>
        /// Determines if a file can be uploaded to a given path.
        /// </summary>
        /// <param name="path">The path to which the file should be uploaded.</param>
        /// <param name="file">The file which should be uploaded.</param>
        /// <returns>true if the upload is allowed, otherwise false.</returns>
        public override bool AuthorizeUpload(string path, HttpPostedFileBase file)
        {
            return CanAccess(path) && IsValidFile(file.FileName);
        }

        private bool IsValidFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var allowedExtensions = Filter.Split(',');

            return allowedExtensions.Any(e => e.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Uploads a file to a given path.
        /// </summary>
        /// <param name="path">The path to which the file should be uploaded.</param>
        /// <param name="file">The file which should be uploaded.</param>
        /// <returns>A <see cref="JsonResult"/> containing the uploaded file's size and name.</returns>
        /// <exception cref="HttpException">Forbidden</exception>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ImageUpload(string path, HttpPostedFileBase file, int id )
        {
            var atomicCheckDTO = _screeningService.GetAtomicCheck(id);
            var atomicCheckImagePath = FileHelper.GetAtomicCheckPictureReportVirtualPath(
                atomicCheckDTO.Screening, atomicCheckDTO);

            path = string.IsNullOrEmpty(path)
                ? _pathProvider.ToAbsolute(atomicCheckImagePath)
                : _pathProvider.CombinePaths(_pathProvider.ToAbsolute(atomicCheckImagePath), path); 
            
            var fileName = Path.GetFileName(file.FileName);

            if (AuthorizeUpload(path, file))
            {
                file.SaveAs(Path.Combine(Server.MapPath(path), fileName));

                return Json(new ImageBrowserEntry
                {
                    Size = file.ContentLength,
                    Name = fileName
                }, "text/plain");
            }
            throw new HttpException(403, "Forbidden");
        }    
    }
}
