CREATE PROCEDURE usp_releases_select
(
	@id Int
)
AS
SELECT
	*
FROM
	releases WITH(NOLOCK)
WHERE
	id = @id
