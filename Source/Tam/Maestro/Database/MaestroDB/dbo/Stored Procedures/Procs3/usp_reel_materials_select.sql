CREATE PROCEDURE usp_reel_materials_select
(
	@id Int
)
AS
SELECT
	*
FROM
	reel_materials WITH(NOLOCK)
WHERE
	id = @id
