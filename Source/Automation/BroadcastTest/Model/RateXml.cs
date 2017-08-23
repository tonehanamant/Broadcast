using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastTest.Model
{
    class RateXml
    {
        ////THIS IS from Paste Special, not very well formed structure. 

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        [System.Xml.Serialization.XmlRootAttribute("AAAA-Message", Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal", IsNullable = false)]
        public partial class AAAAMessage
        {

            private AAAAMessageAAAAValues aAAAValuesField;

            private AAAAMessageProposal proposalField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("AAAA-Values")]
            public AAAAMessageAAAAValues AAAAValues
            {
                get
                {
                    return this.aAAAValuesField;
                }
                set
                {
                    this.aAAAValuesField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposal Proposal
            {
                get
                {
                    return this.proposalField;
                }
                set
                {
                    this.proposalField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageAAAAValues
        {

            private string schemaNameField;

            private string schemaVersionField;

            private string mediaField;

            private string businessObjectField;

            private string actionField;

            private string uniqueMessageIDField;

            /// <remarks/>
            public string SchemaName
            {
                get
                {
                    return this.schemaNameField;
                }
                set
                {
                    this.schemaNameField = value;
                }
            }

            /// <remarks/>
            public string SchemaVersion
            {
                get
                {
                    return this.schemaVersionField;
                }
                set
                {
                    this.schemaVersionField = value;
                }
            }

            /// <remarks/>
            public string Media
            {
                get
                {
                    return this.mediaField;
                }
                set
                {
                    this.mediaField = value;
                }
            }

            /// <remarks/>
            public string BusinessObject
            {
                get
                {
                    return this.businessObjectField;
                }
                set
                {
                    this.businessObjectField = value;
                }
            }

            /// <remarks/>
            public string Action
            {
                get
                {
                    return this.actionField;
                }
                set
                {
                    this.actionField = value;
                }
            }

            /// <remarks/>
            public string UniqueMessageID
            {
                get
                {
                    return this.uniqueMessageIDField;
                }
                set
                {
                    this.uniqueMessageIDField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposal
        {

            private AAAAMessageProposalSeller sellerField;

            private AAAAMessageProposalBuyer buyerField;

            private AAAAMessageProposalAdvertiser advertiserField;

            private string nameField;

            private AAAAMessageProposalOutlets outletsField;

            private AAAAMessageProposalAvailList availListField;

            private System.DateTime endDateField;

            private System.DateTime sendDateTimeField;

            private System.DateTime startDateField;

            private string uniqueIdentifierField;

            private byte versionField;

            private string weekStartDayField;

            /// <remarks/>
            public AAAAMessageProposalSeller Seller
            {
                get
                {
                    return this.sellerField;
                }
                set
                {
                    this.sellerField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalBuyer Buyer
            {
                get
                {
                    return this.buyerField;
                }
                set
                {
                    this.buyerField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAdvertiser Advertiser
            {
                get
                {
                    return this.advertiserField;
                }
                set
                {
                    this.advertiserField = value;
                }
            }

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalOutlets Outlets
            {
                get
                {
                    return this.outletsField;
                }
                set
                {
                    this.outletsField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAvailList AvailList
            {
                get
                {
                    return this.availListField;
                }
                set
                {
                    this.availListField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime endDate
            {
                get
                {
                    return this.endDateField;
                }
                set
                {
                    this.endDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public System.DateTime sendDateTime
            {
                get
                {
                    return this.sendDateTimeField;
                }
                set
                {
                    this.sendDateTimeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime startDate
            {
                get
                {
                    return this.startDateField;
                }
                set
                {
                    this.startDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string uniqueIdentifier
            {
                get
                {
                    return this.uniqueIdentifierField;
                }
                set
                {
                    this.uniqueIdentifierField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public byte version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string weekStartDay
            {
                get
                {
                    return this.weekStartDayField;
                }
                set
                {
                    this.weekStartDayField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalSeller
        {

            private AAAAMessageProposalSellerSalesperson salespersonField;

            private string companyNameField;

            /// <remarks/>
            public AAAAMessageProposalSellerSalesperson Salesperson
            {
                get
                {
                    return this.salespersonField;
                }
                set
                {
                    this.salespersonField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string companyName
            {
                get
                {
                    return this.companyNameField;
                }
                set
                {
                    this.companyNameField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalSellerSalesperson
        {

            private AAAAMessageProposalSellerSalespersonPhone[] phoneField;

            private AAAAMessageProposalSellerSalespersonEmail emailField;

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("Phone")]
            public AAAAMessageProposalSellerSalespersonPhone[] Phone
            {
                get
                {
                    return this.phoneField;
                }
                set
                {
                    this.phoneField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalSellerSalespersonEmail Email
            {
                get
                {
                    return this.emailField;
                }
                set
                {
                    this.emailField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalSellerSalespersonPhone
        {

            private string typeField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalSellerSalespersonEmail
        {

            private string typeField;

            private string valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public string Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalBuyer
        {

            private string buyerNameField;

            private string buyingCompanyNameField;

            /// <remarks/>
            public string BuyerName
            {
                get
                {
                    return this.buyerNameField;
                }
                set
                {
                    this.buyerNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string buyingCompanyName
            {
                get
                {
                    return this.buyingCompanyNameField;
                }
                set
                {
                    this.buyingCompanyNameField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAdvertiser
        {

            private AAAAMessageProposalAdvertiserProduct productField;

            private string nameField;

            /// <remarks/>
            public AAAAMessageProposalAdvertiserProduct Product
            {
                get
                {
                    return this.productField;
                }
                set
                {
                    this.productField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAdvertiserProduct
        {

            private string nameField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalOutlets
        {

            private AAAAMessageProposalOutletsTelevisionStation televisionStationField;

            /// <remarks/>
            public AAAAMessageProposalOutletsTelevisionStation TelevisionStation
            {
                get
                {
                    return this.televisionStationField;
                }
                set
                {
                    this.televisionStationField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalOutletsTelevisionStation
        {

            private string callLettersField;

            private string outletIdField;

            private string parentPlusField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string callLetters
            {
                get
                {
                    return this.callLettersField;
                }
                set
                {
                    this.callLettersField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string outletId
            {
                get
                {
                    return this.outletIdField;
                }
                set
                {
                    this.outletIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string parentPlus
            {
                get
                {
                    return this.parentPlusField;
                }
                set
                {
                    this.parentPlusField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailList
        {

            private string nameField;

            private AAAAMessageProposalAvailListOutletReferences outletReferencesField;

            private AAAAMessageProposalAvailListDemoCategory[] demoCategoriesField;

            private AAAAMessageProposalAvailListTargetDemo targetDemoField;

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriods[] availLineWithDetailedPeriodsField;

            private System.DateTime endDateField;

            private string identifierField;

            private string isPackageField;

            private System.DateTime startDateField;

            /// <remarks/>
            public string Name
            {
                get
                {
                    return this.nameField;
                }
                set
                {
                    this.nameField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAvailListOutletReferences OutletReferences
            {
                get
                {
                    return this.outletReferencesField;
                }
                set
                {
                    this.outletReferencesField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("DemoCategory", IsNullable = false)]
            public AAAAMessageProposalAvailListDemoCategory[] DemoCategories
            {
                get
                {
                    return this.demoCategoriesField;
                }
                set
                {
                    this.demoCategoriesField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAvailListTargetDemo TargetDemo
            {
                get
                {
                    return this.targetDemoField;
                }
                set
                {
                    this.targetDemoField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("AvailLineWithDetailedPeriods")]
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriods[] AvailLineWithDetailedPeriods
            {
                get
                {
                    return this.availLineWithDetailedPeriodsField;
                }
                set
                {
                    this.availLineWithDetailedPeriodsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime endDate
            {
                get
                {
                    return this.endDateField;
                }
                set
                {
                    this.endDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string identifier
            {
                get
                {
                    return this.identifierField;
                }
                set
                {
                    this.identifierField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string isPackage
            {
                get
                {
                    return this.isPackageField;
                }
                set
                {
                    this.isPackageField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime startDate
            {
                get
                {
                    return this.startDateField;
                }
                set
                {
                    this.startDateField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListOutletReferences
        {

            private AAAAMessageProposalAvailListOutletReferencesOutletReference outletReferenceField;

            /// <remarks/>
            public AAAAMessageProposalAvailListOutletReferencesOutletReference OutletReference
            {
                get
                {
                    return this.outletReferenceField;
                }
                set
                {
                    this.outletReferenceField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListOutletReferencesOutletReference
        {

            private string outletForListIdField;

            private string outletFromProposalRefField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string outletForListId
            {
                get
                {
                    return this.outletForListIdField;
                }
                set
                {
                    this.outletForListIdField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string outletFromProposalRef
            {
                get
                {
                    return this.outletFromProposalRefField;
                }
                set
                {
                    this.outletFromProposalRefField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListDemoCategory
        {

            private string demoTypeField;

            private string groupField;

            private byte ageFromField;

            private byte ageToField;

            private string demoIdField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/spotTV")]
            public string DemoType
            {
                get
                {
                    return this.demoTypeField;
                }
                set
                {
                    this.demoTypeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/spotTV")]
            public string Group
            {
                get
                {
                    return this.groupField;
                }
                set
                {
                    this.groupField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/spotTV")]
            public byte AgeFrom
            {
                get
                {
                    return this.ageFromField;
                }
                set
                {
                    this.ageFromField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/spotTV")]
            public byte AgeTo
            {
                get
                {
                    return this.ageToField;
                }
                set
                {
                    this.ageToField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string DemoId
            {
                get
                {
                    return this.demoIdField;
                }
                set
                {
                    this.demoIdField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListTargetDemo
        {

            private string demoRefField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string demoRef
            {
                get
                {
                    return this.demoRefField;
                }
                set
                {
                    this.demoRefField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriods
        {

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsOutletReference outletReferenceField;

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTime[] dayTimesField;

            private string daypartNameField;

            private string availNameField;

            private System.DateTime spotLengthField;

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods periodsField;

            /// <remarks/>
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsOutletReference OutletReference
            {
                get
                {
                    return this.outletReferenceField;
                }
                set
                {
                    this.outletReferenceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("DayTime", IsNullable = false)]
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTime[] DayTimes
            {
                get
                {
                    return this.dayTimesField;
                }
                set
                {
                    this.dayTimesField = value;
                }
            }

            /// <remarks/>
            public string DaypartName
            {
                get
                {
                    return this.daypartNameField;
                }
                set
                {
                    this.daypartNameField = value;
                }
            }

            /// <remarks/>
            public string AvailName
            {
                get
                {
                    return this.availNameField;
                }
                set
                {
                    this.availNameField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(DataType = "time")]
            public System.DateTime SpotLength
            {
                get
                {
                    return this.spotLengthField;
                }
                set
                {
                    this.spotLengthField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods Periods
            {
                get
                {
                    return this.periodsField;
                }
                set
                {
                    this.periodsField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsOutletReference
        {

            private string outletFromListRefField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string outletFromListRef
            {
                get
                {
                    return this.outletFromListRefField;
                }
                set
                {
                    this.outletFromListRefField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTime
        {

            private string startTimeField;

            private string endTimeField;

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimeDays daysField;

            /// <remarks/>
            public string StartTime
            {
                get
                {
                    return this.startTimeField;
                }
                set
                {
                    this.startTimeField = value;
                }
            }

            /// <remarks/>
            public string EndTime
            {
                get
                {
                    return this.endTimeField;
                }
                set
                {
                    this.endTimeField = value;
                }
            }

            /// <remarks/>
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimeDays Days
            {
                get
                {
                    return this.daysField;
                }
                set
                {
                    this.daysField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsDayTimeDays
        {

            private string mondayField;

            private string tuesdayField;

            private string wednesdayField;

            private string thursdayField;

            private string fridayField;

            private string saturdayField;

            private string sundayField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Monday
            {
                get
                {
                    return this.mondayField;
                }
                set
                {
                    this.mondayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Tuesday
            {
                get
                {
                    return this.tuesdayField;
                }
                set
                {
                    this.tuesdayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Wednesday
            {
                get
                {
                    return this.wednesdayField;
                }
                set
                {
                    this.wednesdayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Thursday
            {
                get
                {
                    return this.thursdayField;
                }
                set
                {
                    this.thursdayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Friday
            {
                get
                {
                    return this.fridayField;
                }
                set
                {
                    this.fridayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Saturday
            {
                get
                {
                    return this.saturdayField;
                }
                set
                {
                    this.saturdayField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.AAAA.org/schemas/TVBGeneralTypes")]
            public string Sunday
            {
                get
                {
                    return this.sundayField;
                }
                set
                {
                    this.sundayField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods
        {

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod detailedPeriodField;

            /// <remarks/>
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod DetailedPeriod
            {
                get
                {
                    return this.detailedPeriodField;
                }
                set
                {
                    this.detailedPeriodField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod
        {

            private decimal rateField;

            private AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[] demoValuesField;

            private System.DateTime endDateField;

            private System.DateTime startDateField;

            /// <remarks/>
            public decimal Rate
            {
                get
                {
                    return this.rateField;
                }
                set
                {
                    this.rateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlArrayItemAttribute("DemoValue", IsNullable = false)]
            public AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[] DemoValues
            {
                get
                {
                    return this.demoValuesField;
                }
                set
                {
                    this.demoValuesField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime endDate
            {
                get
                {
                    return this.endDateField;
                }
                set
                {
                    this.endDateField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime startDate
            {
                get
                {
                    return this.startDateField;
                }
                set
                {
                    this.startDateField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.AAAA.org/schemas/spotTVCableProposal")]
        public partial class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue
        {

            private string demoRefField;

            private decimal valueField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string demoRef
            {
                get
                {
                    return this.demoRefField;
                }
                set
                {
                    this.demoRefField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlTextAttribute()]
            public decimal Value
            {
                get
                {
                    return this.valueField;
                }
                set
                {
                    this.valueField = value;
                }
            }
        }




    }
}
