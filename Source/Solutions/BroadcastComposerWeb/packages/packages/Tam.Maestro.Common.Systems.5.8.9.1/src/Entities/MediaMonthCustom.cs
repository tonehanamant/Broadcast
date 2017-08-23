namespace Tam.Maestro.Data.Entities
{
    public partial class MediaMonth
    {
        public int Quarter
        {
            get
            {
                switch (this.Month)
                {
                    case (1):
                    case (2):
                    case (3):
                        return (1);
                    case (4):
                    case (5):
                    case (6):
                        return (2);
                    case (7):
                    case (8):
                    case (9):
                        return (3);
                    case (10):
                    case (11):
                    case (12):
                        return (4);
                }
                return -1;
            }
        }
        public string LongMonthNameAndYear
        {
            get
            {
                return this.LongMonthName + " " + this.Year.ToString();
            }
        }
        public string LongMonthName
        {
            get
            {
                switch (this.Month)
                {
                    case (1):
                        return ("January");
                    case (2):
                        return ("February");
                    case (3):
                        return ("March");
                    case (4):
                        return ("April");
                    case (5):
                        return ("May");
                    case (6):
                        return ("June");
                    case (7):
                        return ("July");
                    case (8):
                        return ("August");
                    case (9):
                        return ("September");
                    case (10):
                        return ("October");
                    case (11):
                        return ("November");
                    case (12):
                        return ("December");
                    default:
                        return ("");
                }
            }
        }
        public string Abbreviation
        {
            get
            {
                switch (this.Month)
                {
                    case (1):
                        return ("Jan");
                    case (2):
                        return ("Feb");
                    case (3):
                        return ("Mar");
                    case (4):
                        return ("Apr");
                    case (5):
                        return ("May");
                    case (6):
                        return ("Jun");
                    case (7):
                        return ("Jul");
                    case (8):
                        return ("Aug");
                    case (9):
                        return ("Sept");
                    case (10):
                        return ("Oct");
                    case (11):
                        return ("Nov");
                    case (12):
                        return ("Dec");
                    default:
                        return ("");
                }
            }
        }
        /// <summary>
        /// Example: 4Q
        /// </summary>
        public string QuarterText
        {
            get
            {
                switch (this.Month)
                {
                    case (1):
                    case (2):
                    case (3):
                        return ("1Q");
                    case (4):
                    case (5):
                    case (6):
                        return ("2Q");
                    case (7):
                    case (8):
                    case (9):
                        return ("3Q");
                    case (10):
                    case (11):
                    case (12):
                        return ("4Q");
                }
                return "";
            }
        }
        /// <summary>
        /// Example: 4Q14
        /// </summary>
        public string QuarterAndYearText
        {
            get
            {
                switch (this.Month)
                {
                    case (1):
                    case (2):
                    case (3):
                        return ("1Q" + this.Year.ToString().Substring(2, 2));
                    case (4):
                    case (5):
                    case (6):
                        return ("2Q" + this.Year.ToString().Substring(2, 2));
                    case (7):
                    case (8):
                    case (9):
                        return ("3Q" + this.Year.ToString().Substring(2, 2));
                    case (10):
                    case (11):
                    case (12):
                        return ("4Q" + this.Year.ToString().Substring(2, 2));
                }
                return "";
            }
        }

    }
}
