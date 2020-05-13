using System;

namespace InventoryProgramEnrichmentIngest
{
    public abstract class ProcessingBaseClass
    {
        protected void _LogInfo(string message)
        {
            Logger.LogInfo(message);
        }

        protected void _LogError(string message, Exception ex)
        {
            Logger.LogError(message, ex);
        }
    }
}