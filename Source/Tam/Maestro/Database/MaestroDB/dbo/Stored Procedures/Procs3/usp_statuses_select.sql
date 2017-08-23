CREATE PROCEDURE usp_statuses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	statuses WITH(NOLOCK)
WHERE
	id = @id
