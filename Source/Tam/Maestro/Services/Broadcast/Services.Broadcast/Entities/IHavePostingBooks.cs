namespace Services.Broadcast.Entities
{
    public interface IHavePostingBooks
    {
        int? SinglePostingBookId { get; set; }
        int? SharePostingBookId { get; set; }
        int? HutPostingBookId { get; set; }
    }
}