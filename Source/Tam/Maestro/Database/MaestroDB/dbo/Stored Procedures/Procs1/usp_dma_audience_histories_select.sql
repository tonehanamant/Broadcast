CREATE PROCEDURE usp_dma_audience_histories_select
(
	@dma_id		Int,
	@audience_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	dma_audience_histories WITH(NOLOCK)
WHERE
	dma_id=@dma_id
	AND
	audience_id=@audience_id
	AND
	start_date=@start_date

