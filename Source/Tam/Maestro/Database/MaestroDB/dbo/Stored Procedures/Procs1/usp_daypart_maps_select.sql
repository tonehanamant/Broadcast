CREATE PROCEDURE usp_daypart_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	daypart_maps WITH(NOLOCK)
WHERE
	id = @id
