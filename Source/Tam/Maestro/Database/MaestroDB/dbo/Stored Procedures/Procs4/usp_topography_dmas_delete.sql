CREATE PROCEDURE usp_topography_dmas_delete
(
	@topography_id		Int,
	@dma_id		Int
)
AS
DELETE FROM topography_dmas WHERE topography_id=@topography_id AND dma_id=@dma_id
