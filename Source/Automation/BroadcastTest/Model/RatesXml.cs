using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BroadcastTest.Model
{
    public class RatesXml
    {
         
	    [XmlRoot(ElementName="AAAA-Values", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class AAAAValues {
		    [XmlElement(ElementName="SchemaName", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string SchemaName { get; set; }
		    [XmlElement(ElementName="SchemaVersion", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string SchemaVersion { get; set; }
		    [XmlElement(ElementName="Media", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string Media { get; set; }
		    [XmlElement(ElementName="BusinessObject", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string BusinessObject { get; set; }
		    [XmlElement(ElementName="Action", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string Action { get; set; }
		    [XmlElement(ElementName="UniqueMessageID", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string UniqueMessageID { get; set; }
	    }

	    [XmlRoot(ElementName="Phone", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Phone {
		    [XmlAttribute(AttributeName="type")]
		    public string Type { get; set; }
		    [XmlText]
		    public string Text { get; set; }
	    }

	    [XmlRoot(ElementName="Email", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Email {
		    [XmlAttribute(AttributeName="type")]
		    public string Type { get; set; }
		    [XmlText]
		    public string Text { get; set; }
	    }

	    [XmlRoot(ElementName="Salesperson", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Salesperson {
		    [XmlElement(ElementName="Phone", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public List<Phone> Phone { get; set; }
		    [XmlElement(ElementName="Email", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Email Email { get; set; }
		    [XmlAttribute(AttributeName="name")]
		    public string Name { get; set; }
	    }

	    [XmlRoot(ElementName="Seller", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Seller {
		    [XmlElement(ElementName="Salesperson", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Salesperson Salesperson { get; set; }
		    [XmlAttribute(AttributeName="companyName")]
		    public string CompanyName { get; set; }
	    }

	    [XmlRoot(ElementName="Buyer", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Buyer {
		    [XmlElement(ElementName="BuyerName", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string BuyerName { get; set; }
		    [XmlAttribute(AttributeName="buyingCompanyName")]
		    public string BuyingCompanyName { get; set; }
	    }

	    [XmlRoot(ElementName="Product", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Product {
		    [XmlAttribute(AttributeName="name")]
		    public string Name { get; set; }
	    }

	    [XmlRoot(ElementName="Advertiser", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Advertiser {
		    [XmlElement(ElementName="Product", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Product Product { get; set; }
		    [XmlAttribute(AttributeName="name")]
		    public string Name { get; set; }
	    }

	    [XmlRoot(ElementName="TelevisionStation", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class TelevisionStation {
		    [XmlAttribute(AttributeName="callLetters")]
		    public string CallLetters { get; set; }
		    [XmlAttribute(AttributeName="outletId")]
		    public string OutletId { get; set; }
		    [XmlAttribute(AttributeName="parentPlus")]
		    public string ParentPlus { get; set; }
	    }

	    [XmlRoot(ElementName="Outlets", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Outlets {
		    [XmlElement(ElementName="TelevisionStation", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public TelevisionStation TelevisionStation { get; set; }
	    }

	    [XmlRoot(ElementName="OutletReference", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class OutletReference {
		    [XmlAttribute(AttributeName="outletForListId")]
		    public string OutletForListId { get; set; }
		    [XmlAttribute(AttributeName="outletFromProposalRef")]
		    public string OutletFromProposalRef { get; set; }
		    [XmlAttribute(AttributeName="outletFromListRef")]
		    public string OutletFromListRef { get; set; }
	    }

	    [XmlRoot(ElementName="OutletReferences", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class OutletReferences {
		    [XmlElement(ElementName="OutletReference", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public OutletReference OutletReference { get; set; }
	    }

	    [XmlRoot(ElementName="DemoCategory", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DemoCategory {
		    [XmlElement(ElementName="DemoType", Namespace="http://www.AAAA.org/schemas/spotTV")]
		    public string DemoType { get; set; }
		    [XmlElement(ElementName="Group", Namespace="http://www.AAAA.org/schemas/spotTV")]
		    public string Group { get; set; }
		    [XmlElement(ElementName="AgeFrom", Namespace="http://www.AAAA.org/schemas/spotTV")]
		    public string AgeFrom { get; set; }
		    [XmlElement(ElementName="AgeTo", Namespace="http://www.AAAA.org/schemas/spotTV")]
		    public string AgeTo { get; set; }
		    [XmlAttribute(AttributeName="DemoId")]
		    public string DemoId { get; set; }
	    }

	    [XmlRoot(ElementName="DemoCategories", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DemoCategories {
		    [XmlElement(ElementName="DemoCategory", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public List<DemoCategory> DemoCategory { get; set; }
	    }

	    [XmlRoot(ElementName="TargetDemo", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class TargetDemo {
		    [XmlAttribute(AttributeName="demoRef")]
		    public string DemoRef { get; set; }
	    }

	    [XmlRoot(ElementName="Days", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Days {
		    [XmlElement(ElementName="Monday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Monday { get; set; }
		    [XmlElement(ElementName="Tuesday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Tuesday { get; set; }
		    [XmlElement(ElementName="Wednesday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Wednesday { get; set; }
		    [XmlElement(ElementName="Thursday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Thursday { get; set; }
		    [XmlElement(ElementName="Friday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Friday { get; set; }
		    [XmlElement(ElementName="Saturday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Saturday { get; set; }
		    [XmlElement(ElementName="Sunday", Namespace="http://www.AAAA.org/schemas/TVBGeneralTypes")]
		    public string Sunday { get; set; }
	    }

	    [XmlRoot(ElementName="DayTime", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DayTime {
		    [XmlElement(ElementName="StartTime", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string StartTime { get; set; }
		    [XmlElement(ElementName="EndTime", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string EndTime { get; set; }
		    [XmlElement(ElementName="Days", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Days Days { get; set; }
	    }

	    [XmlRoot(ElementName="DayTimes", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DayTimes {
		    [XmlElement(ElementName="DayTime", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public List<DayTime> DayTime { get; set; }
	    }

	    [XmlRoot(ElementName="DemoValue", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DemoValue {
		    [XmlAttribute(AttributeName="demoRef")]
		    public string DemoRef { get; set; }
		    [XmlText]
		    public string Text { get; set; }
	    }

	    [XmlRoot(ElementName="DemoValues", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DemoValues {
		    [XmlElement(ElementName="DemoValue", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public List<DemoValue> DemoValue { get; set; }
	    }

	    [XmlRoot(ElementName="DetailedPeriod", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class DetailedPeriod {
		    [XmlElement(ElementName="Rate", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string Rate { get; set; }
		    [XmlElement(ElementName="DemoValues", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public DemoValues DemoValues { get; set; }
		    [XmlAttribute(AttributeName="endDate")]
		    public string EndDate { get; set; }
		    [XmlAttribute(AttributeName="startDate")]
		    public string StartDate { get; set; }
	    }

	    [XmlRoot(ElementName="Periods", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Periods {
		    [XmlElement(ElementName="DetailedPeriod", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public DetailedPeriod DetailedPeriod { get; set; }
	    }

	    [XmlRoot(ElementName="AvailLineWithDetailedPeriods", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class AvailLineWithDetailedPeriods {
		    [XmlElement(ElementName="OutletReference", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public OutletReference OutletReference { get; set; }
		    [XmlElement(ElementName="DayTimes", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public DayTimes DayTimes { get; set; }
		    [XmlElement(ElementName="DaypartName", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string DaypartName { get; set; }
		    [XmlElement(ElementName="AvailName", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string AvailName { get; set; }
		    [XmlElement(ElementName="SpotLength", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string SpotLength { get; set; }
		    [XmlElement(ElementName="Periods", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Periods Periods { get; set; }
	    }

	    [XmlRoot(ElementName="AvailList", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class AvailList {
		    [XmlElement(ElementName="Name", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string Name { get; set; }
		    [XmlElement(ElementName="OutletReferences", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public OutletReferences OutletReferences { get; set; }
		    [XmlElement(ElementName="DemoCategories", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public DemoCategories DemoCategories { get; set; }
		    [XmlElement(ElementName="TargetDemo", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public TargetDemo TargetDemo { get; set; }
		    [XmlElement(ElementName="AvailLineWithDetailedPeriods", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public List<AvailLineWithDetailedPeriods> AvailLineWithDetailedPeriods { get; set; }
		    [XmlAttribute(AttributeName="endDate")]
		    public string EndDate { get; set; }
		    [XmlAttribute(AttributeName="identifier")]
		    public string Identifier { get; set; }
		    [XmlAttribute(AttributeName="isPackage")]
		    public string IsPackage { get; set; }
		    [XmlAttribute(AttributeName="startDate")]
		    public string StartDate { get; set; }
	    }

	    [XmlRoot(ElementName="Proposal", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class Proposal {
		    [XmlElement(ElementName="Seller", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Seller Seller { get; set; }
		    [XmlElement(ElementName="Buyer", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Buyer Buyer { get; set; }
		    [XmlElement(ElementName="Advertiser", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Advertiser Advertiser { get; set; }
		    [XmlElement(ElementName="Name", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public string Name { get; set; }
		    [XmlElement(ElementName="Outlets", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Outlets Outlets { get; set; }
		    [XmlElement(ElementName="AvailList", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public AvailList AvailList { get; set; }
		    [XmlAttribute(AttributeName="endDate")]
		    public string EndDate { get; set; }
		    [XmlAttribute(AttributeName="sendDateTime")]
		    public string SendDateTime { get; set; }
		    [XmlAttribute(AttributeName="startDate")]
		    public string StartDate { get; set; }
		    [XmlAttribute(AttributeName="uniqueIdentifier")]
		    public string UniqueIdentifier { get; set; }
		    [XmlAttribute(AttributeName="version")]
		    public string Version { get; set; }
		    [XmlAttribute(AttributeName="weekStartDay")]
		    public string WeekStartDay { get; set; }
	    }

	    [XmlRoot(ElementName="AAAA-Message", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
	    public class AAAAMessage {
		    [XmlElement(ElementName="AAAA-Values", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public AAAAValues AAAAValues { get; set; }
		    [XmlElement(ElementName="Proposal", Namespace="http://www.AAAA.org/schemas/spotTVCableProposal")]
		    public Proposal Proposal { get; set; }
		    [XmlAttribute(AttributeName="xmlns")]
		    public string Xmlns { get; set; }
		    [XmlAttribute(AttributeName="tvb", Namespace="http://www.w3.org/2000/xmlns/")]
		    public string Tvb { get; set; }
		    [XmlAttribute(AttributeName="tvb-tp", Namespace="http://www.w3.org/2000/xmlns/")]
		    public string Tvbtp { get; set; }
		    [XmlAttribute(AttributeName="xsi", Namespace="http://www.w3.org/2000/xmlns/")]
		    public string Xsi { get; set; }
		    [XmlAttribute(AttributeName="schemaLocation", Namespace="http://www.w3.org/2001/XMLSchema-instance")]
		    public string SchemaLocation { get; set; }
	    }

    }
}

