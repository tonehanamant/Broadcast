using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BroadcastTest.Model
{
    public class DayTime
    {


        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Days DaysOfWeek { get; set; }

        public class Days
        {
            public bool Monday { get; set; }
            public bool Tuesday { get; set; }
            public bool Wednesday { get; set; }
            public bool Thursday { get; set; }
            public bool Friday { get; set; }
            public bool Saturday { get; set; }
            public bool Sunday { get; set; }
        }

        public DayTime()
        {
            this.DaysOfWeek = new Days(); 
        }

        public void SetDay(string day, string yesNo)
        {            

            bool dayVal = false;
            switch (yesNo.ToUpper())
            {
                case "Y": case "YES":
                    dayVal = true;
                    break;
                case "N": case "NO": default:
                    dayVal = false;
                    break;
            }

            switch (day.ToUpper())
            {
                case "MONDAY": case "MON": case "M":
                    this.DaysOfWeek.Monday = dayVal;
                    break;
                case "TUESDAY": case "TUE": case "TUES": case "T":
                    this.DaysOfWeek.Tuesday = dayVal;
                    break;
                case "WEDNESDAY": case "WED": case "W":
                    this.DaysOfWeek.Wednesday = dayVal;
                    break;
                case "THURSDAY": case "THURS": case "TH":
                    this.DaysOfWeek.Thursday = dayVal;
                    break;
                case "FRIDAY": case "FRI": case "F":
                    this.DaysOfWeek.Friday = dayVal;
                    break;
                case "SATURDAY": case "SAT": case "SA":
                    this.DaysOfWeek.Saturday = dayVal;
                    break;
                case "SUNDAY": case "SUN": case "SU":
                    this.DaysOfWeek.Sunday = dayVal;
                    break;
                default:
                    break;
            }
        }

        public string GetAirTime()
        {
            return ConvertDaysToAirTimeRange(DaysOfWeek) + " " + ConvertTime(StartTime) + "-" + ConvertTime(EndTime);
        }


        public DateTime TimeAsDateTime(string time)
        {

            //Convert overage time of day into next day time
            string[] timeParts = Regex.Split(time, ":");
            int hours = Convert.ToInt16(timeParts[0]);
            string mins = timeParts[1]; //Not converting mins

            if (hours >= 24)
            {
                hours = hours - 24;
            }

            string newTime = hours.ToString() + ":" + mins;


            //Debug, or validate time conversion
            /*
            if (!time.Equals(newTime))
            {
                Console.WriteLine("DEBUG: Converted time from XML: " + time + " to a valid DateTime:" + newTime);
            }
            */

            //DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            string pattern = "H:mm";
            return DateTime.ParseExact(newTime, pattern, CultureInfo.InvariantCulture);
        }

        public string ConvertTime(DateTime time)
        {
            //conversion from unknown time -> h:mmtt || htt
            string mins = time.ToString("mm");
            if (mins.Equals("00") || mins.Equals(""))
            {
                //No minutes, format htt
                return time.ToString("htt");
            }
            else
            {
                //Has minutes, format h:mmtt
                return time.ToString("h:mmtt");
            }
        }

        public string ConvertDaysToAirTimeRange(Days days)
        {
            //Inefficient logic, find more effecient way to convert Days to day range used in Air Time.
            if (days.Monday && days.Tuesday && days.Wednesday && days.Thursday && days.Friday && !days.Saturday && !days.Sunday) { return "M-F"; }    //Most frequent match        
            else if (days.Saturday && !days.Monday && !days.Tuesday && !days.Wednesday && !days.Thursday && !days.Friday && !days.Sunday) { return "SA"; }
            else if (days.Sunday && !days.Saturday && !days.Monday && !days.Tuesday && !days.Wednesday && !days.Thursday && !days.Friday) { return "SU"; }
            else if (days.Monday && !days.Tuesday && !days.Wednesday && !days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "M"; }
            else if (days.Tuesday && !days.Monday && !days.Wednesday && !days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "T"; }
            else if (days.Wednesday && !days.Tuesday && !days.Monday && !days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "W"; }
            else if (days.Thursday && !days.Monday && !days.Tuesday && !days.Wednesday && !days.Friday && !days.Saturday && !days.Sunday) { return "TH"; }
            else if (days.Monday && days.Tuesday && !days.Wednesday && !days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "M-T"; }
            else if (days.Monday && days.Tuesday && days.Wednesday && !days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "M-W"; }
            else if (days.Monday && days.Tuesday && days.Wednesday && days.Thursday && !days.Friday && !days.Saturday && !days.Sunday) { return "M-TH"; }
            else if (days.Monday && days.Tuesday && days.Wednesday && days.Thursday && days.Friday && days.Saturday && !days.Sunday) { return "M-SA"; }
            else if (days.Monday && days.Tuesday && days.Wednesday && days.Thursday && days.Friday && days.Saturday && days.Sunday) { return "M-SU"; }
            else return "";
        }
    }
}
