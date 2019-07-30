using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
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
        /// Creates the plan.
        /// </summary>
        /// <param name="newPlan">The new plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the newly created plan</returns>
        int CreatePlan(CreatePlanDto newPlan, string createdBy, DateTime createdDate);

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
    }

    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _PlanRepository;
        private readonly IPlanValidator _PlanValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanService"/> class.
        /// </summary>
        /// <param name="broadcastDataRepositoryFactory">The broadcast data repository factory.</param>
        /// <param name="planValidator">The plan validator.</param>
        public PlanService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IPlanValidator planValidator)
        {
            _PlanRepository = broadcastDataRepositoryFactory.GetDataRepository<IPlanRepository>();
            _PlanValidator = planValidator;
        }

        ///<inheritdoc/>
        public int CreatePlan(CreatePlanDto newPlan, string createdBy, DateTime createdDate)
        {
            _PlanValidator.ValidateNewPlan(newPlan);
            return _PlanRepository.SaveNewPlan(newPlan, createdBy, createdDate);
        }
        
        ///<inheritdoc/>
        public List<LookupDto> GetPlanStatuses()
        {
            return Enum.GetValues(typeof(PlanStatusEnum))
                .Cast<PlanStatusEnum>()
                .Select(x => new LookupDto { Id = (int)x, Display = x.ToString() }).ToList();
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
    }
}
