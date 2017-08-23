CREATE PROCEDURE usp_zone_dmas_select
(
	@zone_id		Int,
	@dma_id		Int
)
AS
SELECT
	*
FROM
	zone_dmas WITH(NOLOCK)
WHERE
	zone_id=@zone_id
	AND
	dma_id=@dma_id

