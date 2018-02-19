namespace Services.Broadcast.Entities
{
    public class ProposalWeekIsciDto
    {
        public int? Id { get; set; }
        public string ClientIsci { get; set; }
        public string HouseIsci { get; set; }
        public string Brand { get; set; }
        public bool MarriedHouseIsci { get; set; }
        public string Days { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
}
