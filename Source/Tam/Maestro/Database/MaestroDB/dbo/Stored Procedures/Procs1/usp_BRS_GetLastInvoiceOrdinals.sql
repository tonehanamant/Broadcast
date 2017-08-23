-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/11/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetLastInvoiceOrdinals]
	@media_month_id INT
AS
BEGIN
	SELECT 
		ct.system_id,
		MAX(cmw_invoice_ordinal)+1 'next_invoice_ordinal' 
	FROM 
		cmw_bills cb
		JOIN cmw_traffic ct (NOLOCK) ON ct.id=cb.cmw_traffic_id 
	WHERE 
		cb.media_month_id=@media_month_id 
	GROUP BY 
		ct.system_id
END
