using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Configuration;
using CVScreeningService.DTO.Client;
using CVScreeningService.DTO.Screening;
using Nalysa.Common.Log;
using WebMatrix.WebData;

namespace CVScreeningWeb.Helpers
{
    public class FileHelper
    {
        /// <summary>
        /// Convert any object to array of byte.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        /// <summary>
        /// Convert stored array of byte to object
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }

        public static string GetFileNameWithoutExtension(string fileName)
        {
            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
                return fileName.Substring(0, fileExtPos);
            return fileName;
        }

        public static string GetFileExtension(string fileName)
        {
            int fileExtPos = fileName.LastIndexOf(".");
            if (fileExtPos >= 0)
                return fileName.Substring(fileExtPos, fileName.Length - fileExtPos);
            return "";
        }

        /// <summary>
        /// Validate uploaded file size is less than 10 MB
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool ValidateFileSize(HttpPostedFileBase file)
        {
            const int maxUploadSize = (10*1024*1024);
            return file.ContentLength <= maxUploadSize;
        }

        /// <summary>
        /// Validate uploaded file size is less than 3 MB
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool ValidateProfilePictureFileSize(HttpPostedFileBase file)
        {
            const int maxUploadSize = (3 * 1024 * 1024);
            return file.ContentLength <= maxUploadSize;
        }

        /// <summary>
        /// Validate content uploaded to the application
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool ValidateContentType(HttpPostedFileBase file)
        {
            return GetExtension().ContainsKey(file.ContentType);
        }

        /// <summary>
        /// Validate content uploaded to the application
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool ValidateImageContentType(HttpPostedFileBase file)
        {
            return GetImageExtension().ContainsKey(file.ContentType);
        }

        /// <summary>
        /// only returns allowed image types 
        /// </summary>
        /// <returns></returns>
        private static IDictionary<string, string> GetImageExtension()
        {
            return new Dictionary<string, string>
            {
                {"image/jpeg", "jpeg"},
                {"image/png", "png"}
            };
        }


        /// <summary>
        /// Content type allowed to be uploaded to the application
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetExtension()
        {
            return new Dictionary<string, string>
            {
                {"application/msword", "doc"},
                {"application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx"},
                {"application/pdf", "pdf"},
                {"image/jpeg", "jpeg"},
                {"image/png", "png"}
            };
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
		public static string BuildCVFileName(HttpPostedFileBase file)
       {
            var user = WebSecurity.CurrentUserId;

            const string format = "yyyyMMdd_HHmmss";
            var timestamp = DateTime.Now.ToString(format);
            var extension = FileHelper.GetFileExtension(file.FileName);

            return string.Format("CV_{0}_{1}{2}", user, timestamp, extension);
        }


        public static string GetFileFolder()
        {
            return "~/" + WebConfigurationManager.AppSettings["FileFolder"];
        }

        /// <summary>
        /// Get relative file path depending of argument value
        /// </summary>
        /// <param name="subFolders"></param>
        /// <returns></returns>
        public static string GetFilePath(params string[] subFolders)
        {
            var path = GetFileFolder();
            foreach (var subFolder in subFolders)
            {
                path += "/" + subFolder;
            }
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="screeningLevelVersion"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static string GenerateScreeningFilePath(ScreeningLevelVersionDTO screeningLevelVersion,
            string fullName)
        {
            var screeningLevel = screeningLevelVersion.ScreeningLevel;
            var contract = screeningLevel.Contract;
            var clientCompany = contract.ClientCompany;

            return GetFilePath(clientCompany.ClientCompanyName,
                contract.ContractReference + "_" + contract.ContractYear,
                screeningLevel.ScreeningLevelName,
                screeningLevelVersion.ScreeningLevelVersionNumber + "", 
                fullName + "_" + DateTime.Now.ToString("MMdd_HHmm"));
        }
        
        /// <summary>
        /// Get screening attachment physical path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetScreeningAttachmentPhysicalPath(ScreeningDTO screeningDTO)
        {
            return screeningDTO.ScreeningPhysicalPath + "/" + WebConfigurationManager.AppSettings["AttachmentFolder"];
        }

        /// <summary>
        /// Get screening attachment virtual path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetScreeningAttachmentVirtualPath(ScreeningDTO screeningDTO)
        {
            return screeningDTO.ScreeningVirtualPath + "/" + WebConfigurationManager.AppSettings["AttachmentFolder"];
        }

        /// <summary>
        /// Get screening physical path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetScreeningReportPhysicalPath(ScreeningDTO screeningDTO)
        {
            return screeningDTO.ScreeningPhysicalPath + "/" + WebConfigurationManager.AppSettings["ReportFolder"];
        }

        /// <summary>
        /// Get screening virtual path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetScreeningReportVirtualPath(ScreeningDTO screeningDTO)
        {
            return screeningDTO.ScreeningVirtualPath + "/" + WebConfigurationManager.AppSettings["ReportFolder"];
        }
        
        /// <summary>
        /// Get atomic check report physical path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetAtomicCheckReportPhysicalPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetScreeningReportPhysicalPath(screeningDTO) + "/" + atomicCheckDTO.AtomicCheckId;
        }

        /// <summary>
        /// Get atomic check report virtual path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <returns></returns>
        public static string GetAtomicCheckReportVirtualPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetScreeningReportVirtualPath(screeningDTO) + "/" + atomicCheckDTO.AtomicCheckId;
        }

        /// <summary>
        /// Get atomic check pictures report folder physical patch
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public static string GetAtomicCheckPictureReportPhysicalPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetAtomicCheckReportPhysicalPath(screeningDTO, atomicCheckDTO) + "/" + WebConfigurationManager.AppSettings["PicturesFolder"];
        }

        /// <summary>
        /// Get atomic check pictures report folder virtual patch
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public static string GetAtomicCheckPictureReportVirtualPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetAtomicCheckReportVirtualPath(screeningDTO, atomicCheckDTO) + "/" + WebConfigurationManager.AppSettings["PicturesFolder"];
        }

        /// <summary>
        /// Get atomic check attachment report folder virtual path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns
        public static string GetAtomicCheckReportAttachmentPhysicalPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetAtomicCheckReportPhysicalPath(screeningDTO, atomicCheckDTO) + "/" + WebConfigurationManager.AppSettings["AttachmentsFolder"];
        }

        /// <summary>
        /// Get atomic check attachment report folder virtual path
        /// </summary>
        /// <param name="screeningDTO"></param>
        /// <param name="atomicCheckDTO"></param>
        /// <returns></returns>
        public static string GetAtomicCheckReportAttachmentVirtualPath(ScreeningDTO screeningDTO, AtomicCheckDTO atomicCheckDTO)
        {
            return GetAtomicCheckReportVirtualPath(screeningDTO, atomicCheckDTO) + "/" + WebConfigurationManager.AppSettings["AttachmentsFolder"];
        }

        /// <summary>
        /// To convert given physicalPath to virtual path
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        public static string PhysicalToVirtualPath(string physicalPath, string hostName)
        {
            var stringList = new Collection<string>();
            foreach (var s in physicalPath.Split('\\').Reverse())
            {
                stringList.Add(s);
                if (s.Equals(WebConfigurationManager.AppSettings["FileFolder"]))
                    break;
            }

            var relativePath = hostName;
            var arrays = stringList.Reverse().ToArray();
            for (var i = 0; i < arrays.Count(); i++)
            {
                relativePath += string.Format(@"{0}", arrays[i]);
                if (i < arrays.Count() - 1)
                {
                    relativePath += @"/";
                }
            }

            LogManager.Instance.Info(String.Format("Relative path: {0}", relativePath));

            return relativePath;
        }

        public static string BuildAttachmentName(string fileName)
        {
/*            var extension = fileName.Split('.')[fileName.Split('.').Count() - 1];
            return fileName.Replace(extension, string.Empty);*/
            return fileName;
        }

        public static string GenerateManualReportFilePath(ScreeningBaseDTO screeningDTO)
        {
            return string.Format("{0}\\Report\\Screening", screeningDTO.ScreeningPhysicalPath);
        }

        public static string GenerateCurrentReportFileName(ScreeningBaseDTO screeningDTO, HttpPostedFileBase file)
        {
            var latestVersion = 0;

            // if any previous version exist
            if (screeningDTO.ScreeningReport.Count > 0)
            {
                latestVersion = (int)screeningDTO.ScreeningReport.Max(e => e.ScreeningReportVersion);
            }
            return string.Format("Report_{0}.{1}", latestVersion, "pdf");
        }
    }
}