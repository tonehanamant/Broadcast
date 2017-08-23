-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/12/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetAdvertisersWithInvoices]
AS
BEGIN
	SELECT DISTINCT 
		ctc.id,
		ctc.[name]
	FROM
		cmw_invoices ci (NOLOCK)
		JOIN cmw_invoice_details cid (NOLOCK) ON cid.cmw_invoice_id=ci.id
		JOIN cmw_traffic ct (NOLOCK) ON ct.id=cid.cmw_traffic_id
		JOIN cmw_traffic_companies ctc (NOLOCK) ON ctc.id=ct.advertiser_cmw_traffic_company_id
	ORDER BY 
		ctc.[name] ASC
END
