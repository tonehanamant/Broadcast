-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/1/2011
-- Description:	
-- =============================================
CREATE PROCEDURE usp_BRS_GetCmwInvoiceDetailsByCmwInvoiceId
	@cmw_invoice_id INT
AS
BEGIN
	SELECT
		cid.*
	FROM
		cmw_invoice_details cid (NOLOCK)
	WHERE
		cid.cmw_invoice_id=@cmw_invoice_id
END
