using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using Tam.Maestro.Common.Utilities.Logging;

namespace Common.Services
{
    public class ValidationExceptionInterceptionBehavior : IInterceptionBehavior
    {
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var result = getNext()(input, getNext);
            if (result.Exception != null)
            {
                var entityValidationException = (result.Exception as DbEntityValidationException);
                if (entityValidationException != null)
                {
                    foreach (var eve in entityValidationException.EntityValidationErrors)
                    {
                        LogHelper.Log.EntityFrameworkEntityError(eve.Entry.Entity.GetType().Name, eve.Entry.State.ToString());
                        foreach (var ve in eve.ValidationErrors)
                        {
                            LogHelper.Log.EntityFrameworkValidationError(ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                }

                var message = result.Exception.Message;
                if (result.Exception.InnerException != null && result.Exception.InnerException.Message != null)
                    message += result.Exception.InnerException.Message;
                throw new Exception(message, result.Exception);
            }
            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute { get { return true; } }
    }
}
