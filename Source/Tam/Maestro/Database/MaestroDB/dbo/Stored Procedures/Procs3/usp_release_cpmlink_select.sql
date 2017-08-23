CREATE PROCEDURE usp_release_cpmlink_select
(
	@id Int
)
AS
SELECT
	*
FROM
	release_cpmlink WITH(NOLOCK)
WHERE
	id = @id
