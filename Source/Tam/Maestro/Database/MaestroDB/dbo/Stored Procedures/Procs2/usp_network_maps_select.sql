CREATE PROCEDURE usp_network_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	network_maps WITH(NOLOCK)
WHERE
	id = @id
