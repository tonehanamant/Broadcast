CREATE PROCEDURE usp_dma_maps_select
(
	@id Int
)
AS
SELECT
	*
FROM
	dma_maps WITH(NOLOCK)
WHERE
	id = @id
