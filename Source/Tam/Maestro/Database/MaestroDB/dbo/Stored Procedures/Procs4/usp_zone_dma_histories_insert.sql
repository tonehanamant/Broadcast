CREATE PROCEDURE usp_zone_dma_histories_insert
(
	@zone_id		Int,
	@dma_id		Int,
	@start_date		DateTime,
	@weight		Float,
	@end_date		DateTime
)
AS
INSERT INTO zone_dma_histories
(
	zone_id,
	dma_id,
	start_date,
	weight,
	end_date
)
VALUES
(
	@zone_id,
	@dma_id,
	@start_date,
	@weight,
	@end_date
)

