using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace InventoryProgramEnrichmentIngest
{


    public static class Logger
    {
        private const string LOG_FILE_NAME = "log.txt";

        public static void LogInfo(string message)
        {
            var timestampString = DateTime.Now.ToString("s");
            _PutToLog($"{timestampString}: {message}");
        }

        public static void LogError(string message, Exception ex)
        {
            var timestampString = DateTime.Now.ToString("s");
            _PutToLog($"{timestampString}: {message} :");
            if (ex != null)
            {
                _PutToLog(ex.ToString());
                _LogInnerExceptions(ex.InnerException);
            }
        }

        private static void _LogInnerExceptions(Exception ex)
        {
            if (ex != null)
            {
                LogError("InnerException", ex);
            }
        }

        public static void CleanupLog()
        {
            if (File.Exists(LOG_FILE_NAME))
            {
                File.Delete(LOG_FILE_NAME);
            }
        }

        private static void _PutToLog(string message)
        {
            Console.WriteLine(message);
            File.AppendAllLines(LOG_FILE_NAME, new List<string>{ message });
        }
    }
}