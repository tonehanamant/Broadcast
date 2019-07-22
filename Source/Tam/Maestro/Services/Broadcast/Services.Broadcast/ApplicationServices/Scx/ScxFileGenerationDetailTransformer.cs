using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;
using System.IO;

namespace Services.Broadcast.ApplicationServices.Scx
{
    /// <summary>
    /// A helpful transformer.
    /// </summary>
    public static class ScxFileGenerationDetailTransformer
    {
        public static ScxFileGenerationDetail TransformFromDtoToEntity(ScxFileGenerationDetailDto dto, IQuarterCalculationEngine quarterCalculator
            , string dropFolderPath)
        {
            var processingStatus = EnumHelper.GetEnum<BackgroundJobProcessingStatus>(dto.ProcessingStatusId);
            var filePath = Path.Combine(dropFolderPath, dto.FileName);
            var quarters = quarterCalculator.GetAllQuartersBetweenDates(dto.StartDateTime, dto.EndDateTime);

            var item = new ScxFileGenerationDetail
            {
                GenerationRequestDateTime = dto.GenerationRequestDateTime,
                GenerationRequestedByUsername = dto.GenerationRequestedByUsername,
                UnitName = dto.UnitName,
                DaypartCodeId = dto.DaypartCodeId,
                DaypartCodeName = dto.DaypartCodeName,
                QuarterDetails = quarters,
                ProcessingStatus = processingStatus,
                FullFilePath = filePath
            };
            return item;
        }
    }
}