using System.Collections.Generic;

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

        public string GenerateFormattedErrorMessage()
        {
            var errorMessage = "";
            if (InvalidLine >= 0)
                errorMessage += $"Record: {InvalidLine + 1}: ";
            if (!string.IsNullOrEmpty(InvalidField))
                errorMessage += $"'{InvalidField}' ";
            errorMessage += this.ErrorMessage + "\r\n";
            return errorMessage;
        }
        public override string ToString()
        {
            string str = "";
            if (!string.IsNullOrEmpty(InvalidField)) str += InvalidField;

            if (!string.IsNullOrEmpty(str))
                str += "::";
            str += this.GetType().FullName;
            return str;
        }



        public static string FormatValidationMessage(List<AffidavitValidationResult> validationErrors)
        {
            string message = "";

            validationErrors.ForEach(v => message += v.GenerateFormattedErrorMessage());
            return message;
        }
    }
}