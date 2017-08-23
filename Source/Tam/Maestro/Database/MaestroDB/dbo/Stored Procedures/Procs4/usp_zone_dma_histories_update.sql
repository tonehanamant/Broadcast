CREATE PROCEDURE usp_zone_dma_histories_update
(
	@zone_id		Int,
	@dma_id		Int,
	@start_date		DateTime,
	@weight		Float,
	@end_date		DateTime
)
AS
UPDATE zone_dma_histories SET
	weight = @weight,
	end_date = @end_date
WHERE
	zone_id = @zone_id AND
	dma_id = @dma_id AND
	start_date = @start_date
