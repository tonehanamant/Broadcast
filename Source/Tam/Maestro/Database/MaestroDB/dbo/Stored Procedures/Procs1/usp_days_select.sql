CREATE PROCEDURE usp_days_select
(
	@id Int
)
AS
SELECT
	*
FROM
	days WITH(NOLOCK)
WHERE
	id = @id
