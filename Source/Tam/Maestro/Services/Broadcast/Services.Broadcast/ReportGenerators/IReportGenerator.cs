using Services.Broadcast.Entities;

namespace Services.Broadcast.ReportGenerators
{
    public interface IReportGenerator<T>
    {
        /// <summary>
        /// Generates a report of type T
        /// </summary>
        /// <param name="dataObject">Data object used to generate the file</param>
        /// <returns>ReportOutput object containing the generated file stream</returns>
        ReportOutput Generate(T dataObject);
    }
}