namespace Services.Broadcast.BusinessEngines
{
    public class AffidavitValidationResult
    {
        public AffidavitValidationResult()
        {
            InvalidLine = -1;
        }
        public bool IsValid { get; set; }
        public string InvalidField { get; set; }
        public int InvalidLine { get; set; }
        public string ErrorMessage { get; set; }
    }
}