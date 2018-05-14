namespace Services.Broadcast.Entities
{
    public interface IHaveSingleSharedPostingBooks
    {
        int? SingleProjectionBookId { get; set; }
        int? ShareProjectionBookId { get; set; }
    }
}