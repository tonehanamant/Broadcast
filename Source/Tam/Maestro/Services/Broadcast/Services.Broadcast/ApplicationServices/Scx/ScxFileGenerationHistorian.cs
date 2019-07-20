using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.ApplicationServices.Scx
{
    /// <summary>
    /// File generation history manager.
    /// </summary>
    public interface IScxFileGenerationHistorian
    {
        /// <summary>
        /// Gets the SCX file generation history.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <returns></returns>
        List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId);
    }

    /// <summary>
    /// File generation history manager.
    /// </summary>
    /// <seealso cref="IScxFileGenerationHistorian" />
    public class ScxFileGenerationHistorian : IScxFileGenerationHistorian
    {
        #region Fields

        private readonly IScxGenerationJobRepository _ScxGenerationJobRepository;
        private readonly IQuarterCalculationEngine _QuarterCalculationEngine;
        private readonly string _DropFolderPath;

        #endregion // #region Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ScxFileGenerationHistorian"/> class.
        /// </summary>
        /// <param name="scxGenerationJobRepository">The SCX generation job repository.</param>
        /// <param name="quarterCalculationEngine">The quarter calculation engine.</param>
        /// <param name="dropFolderPath">The drop folder path.</param>
        public ScxFileGenerationHistorian(IScxGenerationJobRepository scxGenerationJobRepository,
            IQuarterCalculationEngine quarterCalculationEngine,
            string dropFolderPath
            )
        {
            _ScxGenerationJobRepository = scxGenerationJobRepository;
            _QuarterCalculationEngine = quarterCalculationEngine;
            _DropFolderPath = dropFolderPath;
        }

        #endregion // #region Constructor

        #region Operations

        /// <inheritdoc />
        public List<ScxFileGenerationDetail> GetScxFileGenerationHistory(int sourceId)
        {
            var repo = GetScxGenerationJobRepository();
            var detailDtos = repo.GetScxFileGenerationDetails(sourceId);
            var details = TransformFromDtoToEntities(detailDtos);
            return details;
        }

        #endregion // #region Operations

        #region Helpers

        private List<ScxFileGenerationDetail> TransformFromDtoToEntities(List<ScxFileGenerationDetailDto> dtos)
        {
            var calculator = GetQuarterCalculationEngine();
            var dropfolder = GetDropFolderPath();

            var entities = dtos.Select(d =>
            {
                var entity = ScxFileGenerationDetailTransformer.TransformFromDtoToEntity(d, calculator, dropfolder);
                return entity;
            })
            .OrderByDescending(s=> s.GenerationRequestDateTime)
            .ToList();

            return entities;
        }

        /// <summary>
        /// Gets the SCX generation job repository.
        /// </summary>
        /// <returns></returns>
        protected IScxGenerationJobRepository GetScxGenerationJobRepository()
        {
            return _ScxGenerationJobRepository;
        }

        /// <summary>
        /// Gets the quarter calculation engine.
        /// </summary>
        /// <returns></returns>
        protected IQuarterCalculationEngine GetQuarterCalculationEngine()
        {
            return _QuarterCalculationEngine;
        }

        /// <summary>
        /// Gets the drop folder path.
        /// </summary>
        /// <returns></returns>
        protected string GetDropFolderPath()
        {
            return _DropFolderPath;
        }

        #endregion // #region Helpers
    }
}