CREATE PROCEDURE usp_topography_dmas_insert
(
	@topography_id		Int,
	@dma_id		Int,
	@include		Bit,
	@effective_date		DateTime
)
AS
INSERT INTO topography_dmas
(
	topography_id,
	dma_id,
	include,
	effective_date
)
VALUES
(
	@topography_id,
	@dma_id,
	@include,
	@effective_date
)

