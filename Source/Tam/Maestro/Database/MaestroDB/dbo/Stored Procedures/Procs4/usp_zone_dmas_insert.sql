CREATE PROCEDURE usp_zone_dmas_insert
(
	@zone_id		Int,
	@dma_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
INSERT INTO zone_dmas
(
	zone_id,
	dma_id,
	weight,
	effective_date
)
VALUES
(
	@zone_id,
	@dma_id,
	@weight,
	@effective_date
)

