using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace BroadcastTest.Model
{
    //
    // This is still under design. Deciding on XML processing methodology
    //
    class RateDataXml
    {

        public RateDataXml()
        {
        }

        public void DeserializeRateData(string xmlFile)
        {
            try
            {
                RateDataXml xml;
                // Construct an instance of the XmlSerializer with the type  
                // of object that is being deserialized.  
                XmlSerializer mySerializer = new XmlSerializer(typeof(RateDataXml));

                // To read the file, create a FileStream.  
                FileStream fileStream = new FileStream(xmlFile, FileMode.Open);

                // Call the Deserialize method and cast to the object type.  
                xml = (RateDataXml)mySerializer.Deserialize(fileStream);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to deserialize XML file into RateDataXML object. File: " 
                    + xmlFile + "error message: " + e);
            }

        }

        public void SerializeRateData(string xmlFile)
        {}

        public class AaaaMessage
        {
            public string XmlNamespace { get; set; }
            public string xmlnsTvb { get; set; }
            public string xmlnsTvbTp { get; set; }
            public string xmlnsXsi { get; set; }
            public string xsiSchemaLocation { get; set; }

        }

        public class AaaaValues
        {
            public string SchemaName { get; set; }
            public string SchemaVersion { get; set; }
            public string Media { get; set; }
            public string BusinessObject { get; set; }
            public string Action { get; set; }
            public string UniqueMessageID { get; set; }
        }

        public class Proposal
        {
            public string EndDate { get; set; }
            public string StartDate { get; set; }
            public string SendDateTime { get; set; }
            public string UniqueIdentifier { get; set; }
            public string Version { get; set; }
            public string WeekStartDay { get; set; }
        
            public class Seller
            {
                public string CompanyName { get; set; }
                public string ProposalName { get; set; }

                public class SalesPerson
                {
                    public string Name { get; set; }

                    public class Phone 
                    {
                        public string Type { get; set; }
                        public string Number { get; set; }
                    }

                    public class Email
                    {
                        public string Type { get; set; }
                        public string EmailAddress { get; set; }
                    }

                }
                public class Buyer
                {
                    public string BuyingCompanyName { get; set; }
                    public string BuyerName { get; set; }
                }

                public class Advertiser
                {
                    public string Name { get; set; }
                    public string ProductName { get; set; }
                }

                public class Outlets
                {
                    public class TelevisionStation
                    {
                        public string CallLetters { get; set; }
                        public string OutletId { get; set; }
                        public string ParentPlus { get; set; }
                    }
                }
            }

            public class AvailList
            {
                public string EndDate { get; set; }
                public string StartDate { get; set; }
                public string Identifier { get; set; }
                public string IsPackage { get; set; }
                public string Name { get; set; }
                public string TargetDemo { get; set; }

    

                public class OutletReferences
                {
                    public string OutletForListId { get; set; }
                    public string OutletFromProposalRef { get; set; }
                }

                public List<DemoCategory> DemoCategories = new List<DemoCategory>();
 
                public class DemoCategory
                {
                    public string DemoId { get; set; }
                    public string tvbDemoType { get; set; }
                    public string tvbGroup { get; set; }
                    public string tvbAgeFrom { get; set; }
                    public string tvbAgeTo { get; set; }
                }

                [XmlElement("AvailLineWithDetailedPeriods")]
                public List<AvailLineWithDetailedPeriods> availList = new List<AvailLineWithDetailedPeriods>();

                public class AvailLineWithDetailedPeriods
                {
                    public string OutletReference { get; set; }

                    public List<DayTime> DayTimes = new List<DayTime>();

                    public class DayTime
                    {
                        public string StartTime { get; set; }
                        public string EndTime { get; set; }

                        public List<Day> Days = new List<Day>();

                        public class Day
                        {
                            public string Monday { get; set; }
                            public string Tuesday { get; set; }
                            public string Wednesday { get; set; }
                            public string Thursday { get; set; }
                            public string Friday { get; set; }
                            public string Saturday { get; set; }
                            public string Sunday { get; set; }
                        }

                    }

                    public string DayPartName { get; set; }
                    public string AvailName { get; set; }
                    public string SpotLength { get; set; }

                    public List<DetailedPeriod> Periods = new List<DetailedPeriod>();

                    public class DetailedPeriod
                    {
                        public string Rate { get; set; }
                        public string StartDate { get; set; }
                        public string EndDate { get; set; }
                    }

                    public List<DemoValue> DemoValues = new List<DemoValue>();

                    public class DemoValue
                    {
                        public string DemoRef { get; set; }
                        public string Value { get; set; }
                    }

                }

            }

        }


    }
}
