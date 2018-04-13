namespace Services.Broadcast.Entities
{
    public class AffidavitValidationResult
    {
        public AffidavitValidationResult()
        {
            InvalidLine = -1;
        }
        public string InvalidField { get; set; }
        public int InvalidLine { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            string str = "";
            if (!string.IsNullOrEmpty(InvalidField)) str += InvalidField;

            if (!string.IsNullOrEmpty(str))
                str += "::";
            str += this.GetType().FullName;
            return str;
        }
    }
}