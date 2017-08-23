CREATE PROCEDURE usp_topography_dmas_update
(
	@topography_id		Int,
	@dma_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
UPDATE topography_dmas SET
	include = @include,
	effective_date = @effective_date
WHERE
	topography_id = @topography_id AND
	dma_id = @dma_id
