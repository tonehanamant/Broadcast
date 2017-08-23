using Services.Broadcast.Entities;

namespace Services.Broadcast.ReportGenerators
{
    public interface IReportGenerator<T>
    {
        ReportOutput Generate(T file);
    }
}