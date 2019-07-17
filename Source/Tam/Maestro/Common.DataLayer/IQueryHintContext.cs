namespace Tam.Maestro.Common.DataLayer
{
    public interface IQueryHintContext
    {
        string QueryHint { get; set; }
        bool ApplyHint { get; set; }
    }
}
