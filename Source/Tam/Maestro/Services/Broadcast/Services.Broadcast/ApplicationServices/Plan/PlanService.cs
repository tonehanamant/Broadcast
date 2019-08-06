using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Plan
{
    public interface IPlanService : IApplicationService
    {
        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="modifiedBy">The modified by.</param>
        /// <param name="modifiedDate">The modified date.</param>
        /// <returns></returns>
        int SavePlan(PlanDto plan, string modifiedBy, DateTime modifiedDate);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId);

        /// <summary>
        /// Gets the products.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetProducts();

        /// <summary>
        /// Gets the plan statuses.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanStatuses();

        /// <summary>
        /// Gets the delivery types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> GetPlanCurrencies();

        /// <summary>
        /// Calculates the specified plan budget.
        /// </summary>
        /// <param name="planBudget">The plan budget.</param>
        /// <returns>PlanBudgetDeliveryCalculator object</returns>
        PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget);
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;
        private readonly IPlanBudgetDeliveryCalculator _BudgetCalculator;

        private readonly IBroadcastAudiencesCache _AudiencesCache;

        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator
            , IPlanBudgetDeliveryCalculator planBudgetDeliveryCalculator)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanValidator = planValidator;
            _BudgetCalculator = planBudgetDeliveryCalculator;
        }

        ///<inheritdoc/>
        public int SavePlan(PlanDto plan, string modifiedBy, DateTime modifiedDate)
        {
            _PlanValidator.ValidatePlan(plan);

            if (plan.Id == 0)
            {
                return _PlanRepository.SaveNewPlan(plan, modifiedBy, modifiedDate);
            }

            _PlanRepository.SavePlan(plan, modifiedBy, modifiedDate);
            return plan.Id;
        }

        ///<inheritdoc/>
        public PlanDto GetPlan(int planId)
        {
            return _PlanRepository.GetPlan(planId);
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanStatuses()
        {
            return EnumExtensions.ToLookupDtoList<PlanStatusEnum>(); ;
        }

        ///<inheritdoc/>
        public List<LookupDto> GetProducts()
        {
            // PRI-12431 will update this code to return the correct data
            return new List<LookupDto>()
            {
                new LookupDto{ Id = 1, Display = "First product"},
                new LookupDto{ Id = 2, Display = "Second product"},
                new LookupDto{ Id = 3, Display = "Third product"}
            };
        }

        ///<inheritdoc/>
        public List<LookupDto> GetPlanCurrencies()
        {
            return Enum.GetValues(typeof(PlanCurrenciesEnum))
                .Cast<PlanCurrenciesEnum>()
                .Select(x => new LookupDto
                {
                    Id = (int)x,
                    Display = x.GetDescriptionAttribute()
                })
                .OrderBy(x=>x.Id).ToList();
        }

        ///<inheritdoc/>
        public PlanDeliveryBudget Calculate(PlanDeliveryBudget planBudget)
        {
            return _BudgetCalculator.CalculateBudget(planBudget);            
        }
    }
}
