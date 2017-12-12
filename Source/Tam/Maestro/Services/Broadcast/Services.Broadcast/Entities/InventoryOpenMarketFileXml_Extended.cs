namespace Services.Broadcast.Entities.InventoryOpenMarketFileXml
{
    /// <summary>
    /// This is added to deal with:
    ///<Seller companyName="WTVP">
    ///<Salesperson name = "DIANN KARNITSKY" />
    ///< Phone type="voice">(703) 528-7800</Phone>
    ///<Phone type = "fax" > (703) 528-7772</Phone>
    ///<Email type = "primary" > jsmith@coxreps.com</Email>
    ///</Seller>
    /// AND
    ///<Seller companyName="WTVP">
    ///<Salesperson name = "DIANN KARNITSKY">
    ///     < Phone type="voice">(703) 528-7800</Phone>
    ///     <Phone type = "fax" > (703) 528-7772</Phone>
    ///     <Email type = "primary" > jsmith@coxreps.com</Email>
    /// </Salesperson>
    ///</Seller>
    /// </summary>
    public partial class AAAAMessageProposalSeller
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Phone")]
        public AAAAMessageProposalSellerSalespersonPhone[] Phone
        {
            get; set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Email")]
        public AAAAMessageProposalSellerSalespersonEmail[] Email
        {
            get; set;
        }

    }


}

