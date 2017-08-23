CREATE PROCEDURE usp_dma_map_histories_delete
(
	@dma_map_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	dma_map_histories
WHERE
	dma_map_id = @dma_map_id
 AND
	start_date = @start_date
