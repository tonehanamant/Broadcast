CREATE PROCEDURE usp_media_months_select
(
	@id Int
)
AS
SELECT
	*
FROM
	media_months WITH(NOLOCK)
WHERE
	id = @id
