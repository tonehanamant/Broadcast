using System.Data.Entity;

namespace Tam.Maestro.Common.DataLayer
{
    public interface IContextFactory<CT> where CT : DbContext
    {
        CT FromTamConnectionString(string tamConnectionString);
    }
}
