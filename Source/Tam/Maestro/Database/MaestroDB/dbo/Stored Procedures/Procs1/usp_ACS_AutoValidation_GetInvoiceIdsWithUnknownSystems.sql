-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetInvoiceIdsWithUnknownSystems]
	@media_month_id INT
AS
BEGIN
	SELECT	
		i.id
	FROM
		invoices i (NOLOCK)
	WHERE
		i.media_month_id=@media_month_id
		AND (i.system_id IS NULL OR i.invoicing_system_id IS NULL)
END
