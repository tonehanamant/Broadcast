CREATE PROCEDURE usp_dma_maps_select_all
AS
SELECT
	*
FROM
	dma_maps WITH(NOLOCK)
