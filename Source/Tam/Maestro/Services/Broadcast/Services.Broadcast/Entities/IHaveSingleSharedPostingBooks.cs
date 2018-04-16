namespace Services.Broadcast.Entities
{
    public interface IHaveSingleSharedPostingBooks
    {
        int? SinglePostingBookId { get; set; }
        int? SharePostingBookId { get; set; }
    }
}