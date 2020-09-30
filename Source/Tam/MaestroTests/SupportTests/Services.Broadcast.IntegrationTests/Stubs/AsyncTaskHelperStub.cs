using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class AsyncTaskHelperStub : IAsyncTaskHelper
    {
        public List<Action> TaskFireAndForgetActions { get; set; } = new List<Action>();

        public void TaskFireAndForget(Action action)
        {
            TaskFireAndForgetActions.Add(action);
        }
    }
}