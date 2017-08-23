CREATE PROCEDURE usp_dma_histories_select
(
	@dma_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	dma_histories WITH(NOLOCK)
WHERE
	dma_id=@dma_id
	AND
	start_date=@start_date

