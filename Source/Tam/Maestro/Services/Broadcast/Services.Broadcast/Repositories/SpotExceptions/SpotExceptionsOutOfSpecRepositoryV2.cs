using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using System.Data.Entity;
using Services.Broadcast.Entities.ProgramMapping;
using Common.Services.Extensions;

namespace Services.Broadcast.Repositories.SpotExceptions
{
    /// <summary>
    /// Deinfes the interface for method defines for the SpotExceptionsOutOfSpecRepositoryV2
    /// </summary>
    public interface ISpotExceptionsOutOfSpecRepositoryV2 : IDataRepository
    {
        /// <summary>
        /// Get the list of reason codes 
        /// </summary>
        Task<List<SpotExceptionsOutOfSpecReasonCodeDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesV2(int planId, DateTime weekStartDate, DateTime weekEndDate);
    }
    /// <summary>
    /// spot exception repository which interacts with the database
    /// </summary>
    public class SpotExceptionsOutOfSpecRepositoryV2 : BroadcastRepositoryBase, ISpotExceptionsOutOfSpecRepositoryV2
    {
        /// <summary>
        /// constructor of the class
        /// </summary>
        public SpotExceptionsOutOfSpecRepositoryV2(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        { }

        /// <summary>
        /// Get the list of reason codes 
        /// </summary>
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesV2(int planId, DateTime weekStartDate, DateTime weekEndDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var spotExceptionsOutOfSpecReasonCodesEntities = from oos in context.spot_exceptions_out_of_specs
                                                                 join reasoncode in context.spot_exceptions_out_of_spec_reason_codes on
                                                                 oos.reason_code_id equals reasoncode.id into spotexceptionoutofspec
                                                                 where oos.recommended_plan_id == planId && oos.program_air_time >= weekStartDate
                                                                 && oos.program_air_time <= weekEndDate
                                                                 from reasoncode in spotexceptionoutofspec
                                                                 group spotexceptionoutofspec by new { reasoncode.id, reasoncode.reason, reasoncode.reason_code, reasoncode.label } into grouped
                                                                 select new
                                                                 {
                                                                     Id = grouped.Key.id,
                                                                     ReasonCode = grouped.Key.reason_code,
                                                                     Reason = grouped.Key.reason,
                                                                     Label = grouped.Key.label,
                                                                     Count = grouped.Count()
                                                                 };

                var spotExceptionsOutOfSpecReasonCodes = spotExceptionsOutOfSpecReasonCodesEntities.Select(spotExceptionsOutOfSpecReasonCodesEntity => new SpotExceptionsOutOfSpecReasonCodeDtoV2
                {
                    Id = spotExceptionsOutOfSpecReasonCodesEntity.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCodesEntity.ReasonCode,
                    Reason = spotExceptionsOutOfSpecReasonCodesEntity.Reason,
                    Label = spotExceptionsOutOfSpecReasonCodesEntity.Label,
                    Count = spotExceptionsOutOfSpecReasonCodesEntity.Count
                }).ToList();
                return spotExceptionsOutOfSpecReasonCodes;
            });
        }

    }
}
