using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using Tynamix.ObjectFiller;

namespace BroadcastTest.Model
{
    public class RatesData
    {
        public RatesData(){}

        public string StationName { get; set; }
        public string AirTime { get; set; }
        public string Program { get; set; }
        public string Genre { get; set; }
        public string Rate30 { get; set; }
        public string Rate15 { get; set; }
        public string Rate60 { get; set; }
        public string HhImpressions { get; set; }
        public string Rating { get; set; }
        public string Flight { get; set; }
        public string EffectiveDate { get; set; }

        /// <summary>
        /// Print out data for this Rate. This is used for debugging and evaluation purposes. 
        /// </summary>
        public void PrintRatesData_Lines()
        { //Print current instance of object. 
            //Console.WriteLine("Station: " + StationName.ToString());
            if (this.AirTime != null) { Console.WriteLine("Air Time: " + this.AirTime.ToString()); }
            if (this.Program != null) { Console.WriteLine("Program: " + this.Program.ToString()); }
            if (this.Genre != null) { Console.WriteLine("Genre: " + this.Genre.ToString()); }
            if (this.Rate30 != null) { Console.WriteLine("Rate 30: " + this.Rate30.ToString()); }
            if (this.Rate15 != null) { Console.WriteLine("Rate 15: " + this.Rate15.ToString()); }
            if (this.HhImpressions != null) { Console.WriteLine("HH Impressions: " + this.HhImpressions.ToString()); }
            if (this.Rating != null) { Console.WriteLine("Rating: " + this.Rating.ToString()); }
            if (this.Flight != null) { Console.WriteLine("Flight: " + this.Flight.ToString()); }

        }
        public void PrintRatesData()
        { //Print current instance of object. 
            //Console.WriteLine("Station: " + StationName.ToString());
            if (this.AirTime != null) { Console.Write("Air Time: " + this.AirTime.ToString()); }
            if (this.Program != null) { Console.Write(", Program: " + this.Program.ToString()); }
            if (this.Genre != null) { Console.Write(", Genre: " + this.Genre.ToString()); }
            if (this.Rate30 != null) { Console.Write(", Rate 30: " + this.Rate30.ToString()); }
            if (this.Rate15 != null) { Console.Write(", Rate 15: " + this.Rate15.ToString()); }
            if (this.HhImpressions != null) { Console.Write(", HH Impressions: " + this.HhImpressions.ToString()); }
            if (this.Rating != null) { Console.Write(", Rating: " + this.Rating.ToString()); }
            if (this.Flight != null) { Console.Write(", Flight: " + this.Flight.ToString()); }
            Console.WriteLine("");

        }


        //Set flight from start/end date strings.
        public void SetFlight(string startDate, string endDate)
        {

            if (startDate.Equals("") || startDate.Equals("00") || startDate == null)
            {
                startDate = "2016/09/26";  //Default start date for all seed data. Get date from test config/properties.
            }

            this.Flight = ConvertDateFromXls(startDate) + " - " + ConvertDateFromXls(endDate);
        }



        //Following methods are for converting rate data from the XLS file
        public void SetAirTimeFromXls(string dayTime)
        {
            //Valid AirTime format used in UI: "DD-DD HH:MMtt - HH:MMtt"

            string[] lines = Regex.Split(dayTime, "/");  //Split into 'Day' and 'Time' values

            if (lines.Count() > 2 || lines.Equals(""))
            {
                //Do I have correct string? there should be 2 lines only.
                Console.WriteLine("Day/Time may contain multiple day/times or invalid data: " + dayTime);
                //TODO: Throw exception for invalid input.
            }

            //This could fail, but good enough for now (10/18/16).
            //TODO: Improve the XLS -> UI Day/Time conversion. 
            //Convert Day into proper format
            string dayPart = lines[0].ToString(); 
            string startDay = "";
            string endDay = "";
            string day = "";
            if (dayPart.Contains("-"))
            {
                string[] dayRange = Regex.Split(dayPart, "-");
                if (dayRange.Length == 2) //Valid days range, otherwise single day
                {
                    startDay = dayRange[0];
                    endDay = dayRange[1];
                }

                day = startDay + "-" + endDay;
            }
            else //Single day given
            {
                day = dayPart;
            }

            //Convert Time into proper format
            string timePart = lines[1].ToString(); //Time
            string startTime = "";
            string startTimeAmPm = "";
            string endTime = "";
            string endTimeAmPm = "";

            if (timePart.Contains("-"))
            {
                //Extract start/end times
                string[] timeRange = Regex.Split(lines[1].ToString(), "-");
                startTime = timeRange[0].ToString();
                endTime = timeRange[1].ToString();
            }


            //Extract Am/Pm from each time, the end time sometimes has am/pm for first value
            startTimeAmPm = ConvertAmPmFromXls(Regex.Replace(startTime, @"\d", ""));
            endTimeAmPm = ConvertAmPmFromXls(Regex.Replace(endTime, @"\d", ""));
            endTime = ConvertTimeFromXls(Regex.Replace(endTime, @"\D", ""));
            startTime = ConvertTimeFromXls(Regex.Replace(startTime, @"\D", ""));

            if (startTimeAmPm.Equals(""))
            {
                startTimeAmPm = endTimeAmPm; //Am/Pm indicated in range end time and not provided on start time. 
            }

            this.AirTime = day + " " + startTime + startTimeAmPm + "-" + endTime + endTimeAmPm;

            //Debugging Output
            //Console.WriteLine("Extracted Rate AirTime from XLS Day/Time: day=" + day + ", start_time=" + startTime + ", end_time=" + endTime + ".");
            //Console.WriteLine("AirTime stored as: " + this.AirTime);


        }

        /// <summary>
        /// Convert a time hours value from XLS Day/Time part into AirTime 
        /// </summary>
        /// <param name="time">String containing hours and mins in some string format used in XLS.</param>
        /// <returns></returns>
        public string ConvertTimeFromXls(string time)
        {
            //ensure we have only numbers, no :
            if (time.Length > 4)
            {
                //invalid, we only accept HHMM, HMM, H
                Console.WriteLine("Invalid Time - Can't convert time with more than 4 digits." + time);
            }
            if (time.Length == 4) //HHMM
            {
                //return HH:MM
                return time.Insert(2, ":");
            }
            if (time.Length == 3) //HMM
            {
                //return H:MM
                return time.Insert(1, ":");
            }
            if (time.Length <= 2 && time.Length >= 0) //HH
            {
                return time; //don't touch, valid time format
            }
            if (time.Length <= 0)
            {
                //Invalid, the time isn't a time. 
                Console.WriteLine("Invalid Time - Can't convert time with less than 1 digit." + time);
            }

            return time; //Invalid, the time isn't a valid time. TODO: Figure how to handle.
        }

        /// <summary>
        /// Parse Am/Pm values from Xls into format used in the UI for evaluation purposes.
        /// Use this to allow this data to model the UI formatting exactly.
        /// </summary>
        /// <param name="amPm"></param>
        /// <returns></returns>
        public string ConvertAmPmFromXls(string amPm)
        {
            switch (amPm.ToUpper())
            {
                case "A":
                    return "AM";
                case "P":
                    return "PM";                    
                case "N":
                    return "PM";                    
                default:
                    return amPm; //Nothing to convert, return unconverted string.                  
            }
        }

        /// <summary>
        /// Convert the start/end dates from XLS into format used in flight dates column.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string ConvertDateFromXls(string date)
        {
            //conversion from MMMdd/yy -> yyyy/MM/dd

            string pattern = "yyyy/MM/dd";  //valid format
            DateTime parsedDate = Convert.ToDateTime(date);
            string newDate = parsedDate.ToString(pattern);

            //DEBUG
            //Console.WriteLine("Converted XLS Date:" + date + " to Flight Date: " + newDate);
     
            return newDate;

        }

        /// <summary>
        /// Convert the day code from XLS into day format used in AirTime column. 
        /// Use this to allow this data to model the UI formatting exactly.
        /// </summary>
        /// <param name="fromDay"></param>
        /// <returns></returns>
        public string ConvertDayCodeFromXls(string fromDay)
        {

            switch(fromDay.ToUpper())
            {
                case "SUNDAY": case "SUN": 
                    return "SU";
                case "MONDAY": case "MON":
                    return "M";
                case "TUESDAY": case "TUE": case "TUES":
                    return "T";
                case "WEDNESDAY": case "WED":
                    return "W";
                case "THURSDAY": case "THURS":
                    return "TH";
                case "FRIDAY": case "FRI":
                    return "F";
                case "SATURDAY":
                    return "SA";
                default:
                    return fromDay; //Nothing to convert, return unconverted string.   
            } 
        }


        public string ConvertHhImpressionsFromUI(string uiValue)
        {
            return uiValue.Replace(',', '.');
        }

        public string ConvertHhImpressionsToUI(string hhVal)
        {
            string newVal = HhImpressions.Replace('.', ',');
            string r = newVal.Substring(0, hhVal.Length - 1);
            return hhVal.Replace(',', '.') + "0"; ;
        }


    }

    //OBJECT FILLER - Generate random Rate Data record using existing station and program names. 
    //Note: May need to check if record exists against DB or UI before populating XML to import
    public class RateDataRandomFiller
    {
        public void CreateStationRateDataForExistingProgram(string stationName, string programName)
        {
            RatesData rate = new RatesData();
            
            Filler<RatesData> rateFiller = new Filler<RatesData>();
            rateFiller.Setup() 
                .OnProperty(x => x.StationName).Use(stationName)    //TODO: Use existing station name     
                .OnProperty(x => x.Program).Use(programName)             //TODO: Use existing program name
                .OnProperty(x => x.AirTime).Use(GetAvailableAirTime(stationName, programName))    //TODO: Generate random airtime not yet used in station for program  
                .OnProperty(x => x.Genre).Use("")           //TODO: TBD until BCOP-186, not sure what Genre looks like yet, use ""
                .OnProperty(x => x.Rate15).Use("-")     //TODO: Use '-' or valid rate $XXX.XX
                .OnProperty(x => x.Rate30).Use("$" + new IntRange(0, 10000) + ".00")      //TODO: Use '-' or valid rate $XXX.XX
                .OnProperty(x => x.HhImpressions).Use(new IntRange(1, 50).ToString() + "." + new IntRange(0, 9).ToString() + "0")
                .OnProperty(x => x.Rating).Use(new IntRange(0,5).ToString() + "." + new IntRange(1,9).ToString() + "0")
                .OnProperty(x => x.Flight).Use("2016/09/26 - 2016/12/25");


            RatesData r = rateFiller.Create();
         }

        public string GetAvailableAirTime(string stationName, string programName)
        {
            //Get list of all air-times for this station/program 
            //Generate random air-time that is not used by the station/program
            return "M-TH 8PM-10:30PM";
        }


        public void FillRate()
        {
            RatesData rate = new RatesData();

            Filler<RatesData> rateFiller = new Filler<RatesData>();
            RatesData r = rateFiller.Fill(rate);
        }
    }


}
