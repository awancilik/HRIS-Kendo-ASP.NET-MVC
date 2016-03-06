using System;
using System.Diagnostics;
using System.IO;
using CVScreeningService.Filters;
using CVScreeningService.Services.Screening;

namespace CVScreeningService.Services.Reporting
{
    [Logging(Order = 1), ExceptionHandling(Order = 2)]
    public class PDFConverter : IPDFConverter
    {
        //Define where we put the wkhtmltopdf executable file in HtmlToPdfExePath
        private const string HtmlToPdfExePath = @"C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe";
        private readonly IScreeningService _screeningService;

        public PDFConverter(IScreeningService screeningService)
        {
            _screeningService = screeningService;
        }

        public virtual byte[] Convert(string source, string commandLocation, int screeningId, string hostName)
        {
            // retrieve screening information
            var screening = _screeningService.GetScreening(screeningId);

            var psi = new ProcessStartInfo {FileName = Path.Combine(commandLocation, HtmlToPdfExePath)};
            // ReSharper disable once AssignNullToNotNullAttribute
            psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);

            // run the conversion utility
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            // note: that we tell wkhtmltopdf to be quiet and not run scripts
            var args = "-q -n ";
            
            // Global configuration
            args += "--disable-smart-shrinking ";
            args += "--outline-depth 0 ";
            args += "--page-size A4 ";
            args += "--margin-bottom 25.4mm ";
            args += "--margin-left 10mm ";
            args += "--margin-right 10mm ";
            args += "--margin-top 25.4mm ";
            args += "--title \"CV Screening\" ";
            
            // Footer configuration
            args += "--footer-left \"Page [page] of [toPage]\" ";
            args += "--footer-spacing 12 ";
            args += "--footer-font-size 10 ";
            args += "--footer-line ";

            // Header configuration
            args += "--header-right \""
                + string.Format("Pre-employment screening – {0} – {1}",
                screening.ScreeningFullName,
                DateTime.Now.ToString("dd MMMM yyyy")) + "\" ";
            args += "--header-spacing 12 ";
            args += "--header-font-size 10 ";
            args += "--header-line ";
            
            // Cover configuration refering to http://hostname/Report/CoverPage/screeningId
            args += "cover " + hostName 
                + "Report/CoverPage/" + screeningId + " ";
            
            // Table of content configuration
            args += "toc --xsl-style-sheet default.xsl ";

            args += " - -";
            psi.Arguments = args;
            var process = Process.Start(psi);

            try
            {
                using (var stdin = process.StandardInput)
                {
                    stdin.AutoFlush = true;
                    stdin.Write(source);
                }

                //read output
                var buffer = new byte[32768];
                byte[] file;
                using (var ms = new MemoryStream())
                {
                    while (true)
                    {
                        var read = process.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            break;
                        ms.Write(buffer, 0, read);
                    }
                    file = ms.ToArray();
                }

                process.StandardOutput.Close();
                // wait or exit
                process.WaitForExit(60000);

                // read the exit code, close process
                var returnCode = process.ExitCode;
                process.Close();

                if (returnCode == 0)
                    return file;
            }
            catch (Exception)
            {

            }
            finally
            {
                process.Close();
                process.Dispose();
            }
            return null;
        }
    }
}