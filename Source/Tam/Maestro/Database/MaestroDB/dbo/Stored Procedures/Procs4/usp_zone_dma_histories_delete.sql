CREATE PROCEDURE usp_zone_dma_histories_delete
(
	@zone_id		Int,
	@dma_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	zone_dma_histories
WHERE
	zone_id = @zone_id
 AND
	dma_id = @dma_id
 AND
	start_date = @start_date
