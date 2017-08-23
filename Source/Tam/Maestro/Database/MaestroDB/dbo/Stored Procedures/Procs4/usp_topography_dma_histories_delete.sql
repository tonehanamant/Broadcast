CREATE PROCEDURE usp_topography_dma_histories_delete
(
	@topography_id		Int,
	@dma_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	topography_dma_histories
WHERE
	topography_id = @topography_id
 AND
	dma_id = @dma_id
 AND
	start_date = @start_date
