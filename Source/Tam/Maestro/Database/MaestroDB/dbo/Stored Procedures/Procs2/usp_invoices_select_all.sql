CREATE PROCEDURE [dbo].[usp_invoices_select_all]
AS
BEGIN
SELECT
	*
FROM
	dbo.invoices WITH(NOLOCK)
END
