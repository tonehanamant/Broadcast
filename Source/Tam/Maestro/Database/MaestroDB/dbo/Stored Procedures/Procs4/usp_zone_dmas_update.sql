CREATE PROCEDURE usp_zone_dmas_update
(
	@zone_id		Int,
	@dma_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
UPDATE zone_dmas SET
	weight = @weight,
	effective_date = @effective_date
WHERE
	zone_id = @zone_id AND
	dma_id = @dma_id
