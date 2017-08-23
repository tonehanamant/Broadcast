CREATE PROCEDURE usp_activities_select
(
	@id Int
)
AS
SELECT
	*
FROM
	activities WITH(NOLOCK)
WHERE
	id = @id
