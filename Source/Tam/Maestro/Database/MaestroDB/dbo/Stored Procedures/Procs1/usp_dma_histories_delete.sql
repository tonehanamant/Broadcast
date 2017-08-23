CREATE PROCEDURE usp_dma_histories_delete
(
	@dma_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	dma_histories
WHERE
	dma_id = @dma_id
 AND
	start_date = @start_date
