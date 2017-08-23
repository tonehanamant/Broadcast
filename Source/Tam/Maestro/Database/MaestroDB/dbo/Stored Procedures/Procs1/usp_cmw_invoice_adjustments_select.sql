CREATE PROCEDURE usp_cmw_invoice_adjustments_select
(
	@id Int
)
AS
SELECT
	*
FROM
	cmw_invoice_adjustments WITH(NOLOCK)
WHERE
	id = @id
