


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_WEB_GetOperatorStatementOfAccountReport 
CREATE PROCEDURE [dbo].usp_WEB_GetInvoiceYears
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		DISTINCT mm.year
	FROM
		invoices i (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=i.media_month_id
	ORDER BY
		mm.year DESC
END



