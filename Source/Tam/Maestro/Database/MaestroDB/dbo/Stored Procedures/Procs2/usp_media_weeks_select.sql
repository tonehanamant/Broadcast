CREATE PROCEDURE usp_media_weeks_select
(
	@id Int
)
AS
SELECT
	*
FROM
	media_weeks WITH(NOLOCK)
WHERE
	id = @id
