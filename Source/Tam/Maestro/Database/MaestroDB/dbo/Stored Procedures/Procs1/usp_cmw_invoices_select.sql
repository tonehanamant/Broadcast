CREATE PROCEDURE usp_cmw_invoices_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_invoices WITH(NOLOCK)
WHERE
	id = @id
