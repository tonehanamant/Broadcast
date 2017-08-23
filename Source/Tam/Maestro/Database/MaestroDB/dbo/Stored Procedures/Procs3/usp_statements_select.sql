CREATE PROCEDURE usp_statements_select
(
	@id Int
)
AS
SELECT
	*
FROM
	statements WITH(NOLOCK)
WHERE
	id = @id
