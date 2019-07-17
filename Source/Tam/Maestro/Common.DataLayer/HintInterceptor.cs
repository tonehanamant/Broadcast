using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace Tam.Maestro.Common.DataLayer
{
    public class HintInterceptor : DbCommandInterceptor
    {
        public override void ReaderExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            if (interceptionContext.DbContexts.Any(db => db is IQueryHintContext))
            {
                var ctx = interceptionContext.DbContexts.First(db => db is IQueryHintContext) as IQueryHintContext;
                if (ctx.ApplyHint)
                {
                    command.CommandText += string.Format(" option ({0})", ctx.QueryHint);
                }
            }
            base.ReaderExecuting(command, interceptionContext);
        }
    }
}
