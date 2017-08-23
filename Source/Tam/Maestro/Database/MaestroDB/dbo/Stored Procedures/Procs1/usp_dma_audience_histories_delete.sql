CREATE PROCEDURE usp_dma_audience_histories_delete
(
	@dma_id		Int,
	@audience_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	dma_audience_histories
WHERE
	dma_id = @dma_id
 AND
	audience_id = @audience_id
 AND
	start_date = @start_date
