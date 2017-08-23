CREATE PROCEDURE usp_topography_dma_histories_insert
(
	@topography_id		Int,
	@dma_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
INSERT INTO topography_dma_histories
(
	topography_id,
	dma_id,
	start_date,
	include,
	end_date
)
VALUES
(
	@topography_id,
	@dma_id,
	@start_date,
	@include,
	@end_date
)

