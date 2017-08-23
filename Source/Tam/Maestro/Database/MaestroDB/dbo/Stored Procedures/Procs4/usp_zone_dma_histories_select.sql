CREATE PROCEDURE usp_zone_dma_histories_select
(
	@zone_id		Int,
	@dma_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	zone_dma_histories WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	dma_id=@dma_id
	AND
	start_date=@start_date

