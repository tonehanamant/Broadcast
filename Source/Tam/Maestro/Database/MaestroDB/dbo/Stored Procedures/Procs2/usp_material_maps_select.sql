CREATE PROCEDURE usp_material_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	material_maps WITH(NOLOCK)
WHERE
	id = @id
