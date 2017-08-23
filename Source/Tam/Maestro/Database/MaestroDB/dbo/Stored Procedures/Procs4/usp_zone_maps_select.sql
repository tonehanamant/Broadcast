CREATE PROCEDURE usp_zone_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	zone_maps WITH(NOLOCK)
WHERE
	id = @id
