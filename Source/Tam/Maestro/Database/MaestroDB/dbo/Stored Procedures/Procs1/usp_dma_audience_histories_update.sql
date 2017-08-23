CREATE PROCEDURE usp_dma_audience_histories_update
(
	@dma_id		Int,
	@audience_id		Int,
	@start_date		DateTime,
	@universe		Int,
	@end_date		DateTime
)
AS
UPDATE dma_audience_histories SET
	universe = @universe,
	end_date = @end_date
WHERE
	dma_id = @dma_id AND
	audience_id = @audience_id AND
	start_date = @start_date
