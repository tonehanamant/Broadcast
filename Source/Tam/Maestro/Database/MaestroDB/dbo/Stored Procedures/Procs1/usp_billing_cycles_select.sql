CREATE PROCEDURE usp_billing_cycles_select
(
	@id Int
)
AS
SELECT
	*
FROM
	billing_cycles WITH(NOLOCK)
WHERE
	id = @id
