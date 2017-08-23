
CREATE PROCEDURE [dbo].[usp_invoice_types_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	invoice_types WITH(NOLOCK)
WHERE
	id = @id

