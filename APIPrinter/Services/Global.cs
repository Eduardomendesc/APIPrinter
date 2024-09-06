using System;
using System.IO;

namespace APIPrinter.Services
{
    public class Global
    {
        private readonly IWebHostEnvironment _env;

        public Global(IWebHostEnvironment env)
        {
            _env = env;
        }

        public bool LogFile(string sExceptionName, int nErrorLineNo, string sScreen, string sMessage)
        {
            // Define the folder path (you can change WebRootPath to ContentRootPath if needed)
            string pastaFicheiros = Path.Combine(_env.WebRootPath, "Logs");

            // Create the directory if it doesn't exist
            if (!Directory.Exists(pastaFicheiros))
            {
                Directory.CreateDirectory(pastaFicheiros);
            }

            // Create the log file name
            string sFileName = Path.Combine(pastaFicheiros, "logfile.txt");

            // Open or create the log file
            StreamWriter log;
            if (!File.Exists(sFileName))
            {
                log = new StreamWriter(sFileName);
            }
            else
            {
                log = File.AppendText(sFileName);
            }

            // Write the log information
            log.WriteLine("Exception Name : " + sExceptionName);
            log.WriteLine("Date Time      : " + DateTime.Now);
            log.WriteLine("Error Line No. : " + nErrorLineNo);
            log.WriteLine("Form Name      : " + sScreen);
            log.WriteLine("sMessage       : " + sMessage);
            log.WriteLine("=================================================================");
            log.Close();

            return true;
        }

    }
}
