CREATE PROCEDURE usp_zone_dmas_delete
(
	@zone_id		Int,
	@dma_id		Int
)
AS
DELETE FROM zone_dmas WHERE zone_id=@zone_id AND dma_id=@dma_id
