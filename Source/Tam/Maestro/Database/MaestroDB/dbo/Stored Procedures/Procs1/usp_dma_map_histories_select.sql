CREATE PROCEDURE usp_dma_map_histories_select
(
	@dma_map_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	dma_map_histories WITH(NOLOCK)
WHERE
	dma_map_id=@dma_map_id
	AND
	start_date=@start_date

