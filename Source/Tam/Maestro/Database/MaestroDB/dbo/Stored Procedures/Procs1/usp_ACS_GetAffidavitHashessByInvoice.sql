-- =============================================
-- Author:		Stephen DeFusco
-- Create date: ?
-- Update date:	12/4/2012
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetAffidavitHashessByInvoice]
	@invoice_id INT
AS
BEGIN
	DECLARE @media_month_id INT
	SELECT @media_month_id = i.media_month_id FROM invoices i (NOLOCK) WHERE i.id=@invoice_id
	
	SELECT 
		a.invoice_id, 
		a.hash 
	FROM 
		affidavits a (NOLOCK) 
	WHERE 
		a.media_month_id=@media_month_id
		AND a.invoice_id=@invoice_id
END
