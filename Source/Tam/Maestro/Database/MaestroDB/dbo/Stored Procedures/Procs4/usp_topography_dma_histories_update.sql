CREATE PROCEDURE usp_topography_dma_histories_update
(
	@topography_id		Int,
	@dma_id		Int,
	@start_date		DateTime,
	@include		Bit,
	@end_date		DateTime
)
AS
UPDATE topography_dma_histories SET
	include = @include,
	end_date = @end_date
WHERE
	topography_id = @topography_id AND
	dma_id = @dma_id AND
	start_date = @start_date
