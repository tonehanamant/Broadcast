CREATE PROCEDURE usp_logs_select
(
	@id Int
)
AS
SELECT
	*
FROM
	logs WITH(NOLOCK)
WHERE
	id = @id
