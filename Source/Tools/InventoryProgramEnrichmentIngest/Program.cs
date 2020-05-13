using System;
using System.Diagnostics;

namespace InventoryProgramEnrichmentIngest
{
    class Program
    {
        static void Main(string[] args)
        {
            var totalDurationSw = new Stopwatch();
            totalDurationSw.Start();

            _LogStart();
            _LogInfo("Starting InventoryProgramEnrichmentIngest");
            var processConfig = new ProcessConfiguration();
            try
            {
                processConfig.Load();

                var processingEngine = new ProcessEngine();
                processingEngine.PerformFileProcessing(processConfig);
            }
            catch (Exception ex)
            {
                _LogError("Error caught in main process.", ex);
            }
            finally
            {
                totalDurationSw.Stop();
            }
            _LogInfo($"Total Duration : {totalDurationSw.ElapsedMilliseconds}ms");

            _LogInfo("All done!");

            if (processConfig.ShouldPauseOnDone)
            {
                Console.WriteLine("Hit <ENTER> to end...");
                Console.ReadLine();
            }
        }

        static void _LogStart()
        {
            Logger.CleanupLog();
        }

        static void _LogInfo(string message)
        {
            Logger.LogInfo(message);
        }

        static void _LogError(string message, Exception ex)
        {
            Logger.LogError(message, ex);
        }
    }
}
