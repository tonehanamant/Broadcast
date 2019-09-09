using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    /// <summary>
    /// Mocked for unit testing.
    /// Because we use .Enqueue(...) which is an extension method.
    /// </summary>
    /// <seealso cref="Hangfire.IBackgroundJobClient" />
    public class MockedHangfireBackgroundJobClient : IBackgroundJobClient
    {
        public Queue<Job> Jobs { get; } = new Queue<Job>();

        public string Create(Job job, IState state)
        {
            Jobs.Enqueue(job);
            return Jobs.Count.ToString();
        }

        public bool ChangeState(string jobId, IState state, string expectedState)
        {
            throw new NotImplementedException();
        }
    }
}