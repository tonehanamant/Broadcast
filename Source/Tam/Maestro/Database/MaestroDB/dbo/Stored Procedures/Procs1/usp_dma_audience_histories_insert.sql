CREATE PROCEDURE usp_dma_audience_histories_insert
(
	@dma_id		Int,
	@audience_id		Int,
	@start_date		DateTime,
	@universe		Int,
	@end_date		DateTime
)
AS
INSERT INTO dma_audience_histories
(
	dma_id,
	audience_id,
	start_date,
	universe,
	end_date
)
VALUES
(
	@dma_id,
	@audience_id,
	@start_date,
	@universe,
	@end_date
)

