-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_ACS_AutoValidation_GetAffidavitCountForInvoice 382,'15066295'
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetAffidavitCountForInvoice]
	@media_month_id INT,
	@invoice_ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		a.invoice_id,
		i.date_created,
		COUNT(a.id)
	FROM 
		affidavits a		(NOLOCK)
		JOIN invoices i		(NOLOCK) ON i.id=a.invoice_id
	WHERE 
		a.media_month_id=@media_month_id
		AND a.invoice_id IN (
			SELECT id FROM dbo.SplitIntegers(@invoice_ids)
		)
	GROUP BY
		a.invoice_id,
		i.date_created
	ORDER BY
		i.date_created DESC
END
