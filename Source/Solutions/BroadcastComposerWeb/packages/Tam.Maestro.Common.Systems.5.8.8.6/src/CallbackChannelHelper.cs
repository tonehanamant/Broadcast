using System.ServiceModel;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services
{
    public interface ICallbackChannelHelper
    {
        Tam.Maestro.Services.ContractInterfaces.IBOMSCallback GetCallback();
    }

    public class CallbackChannelHelper : ICallbackChannelHelper
    {
        public Tam.Maestro.Services.ContractInterfaces.IBOMSCallback GetCallback()
        {
            return OperationContext.Current.GetCallbackChannel<IBOMSCallback>();
        }
    }
}
