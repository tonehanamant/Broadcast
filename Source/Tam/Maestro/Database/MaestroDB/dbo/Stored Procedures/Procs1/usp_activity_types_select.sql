CREATE PROCEDURE usp_activity_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	activity_types WITH(NOLOCK)
WHERE
	id = @id
