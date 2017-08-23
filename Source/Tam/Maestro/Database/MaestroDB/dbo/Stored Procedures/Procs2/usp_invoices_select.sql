CREATE PROCEDURE [dbo].[usp_invoices_select]
(
	@id Int
)
AS
BEGIN
SELECT
	*
FROM
	dbo.invoices WITH(NOLOCK)
WHERE
	id = @id
END
