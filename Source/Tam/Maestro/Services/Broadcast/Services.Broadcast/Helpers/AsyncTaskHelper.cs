using System;
using System.Threading.Tasks;

namespace Services.Broadcast.Helpers
{
    public interface IAsyncTaskHelper
    {
        void TaskFireAndForget(Action action);
    }

    public class AsyncTaskHelper : IAsyncTaskHelper
    {
        public void TaskFireAndForget(Action action)
        {
            Task.Run(action);
        }
    }
}