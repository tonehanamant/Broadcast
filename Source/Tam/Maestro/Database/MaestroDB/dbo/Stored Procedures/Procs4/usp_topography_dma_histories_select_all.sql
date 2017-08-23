CREATE PROCEDURE usp_topography_dma_histories_select_all
AS
SELECT
	*
FROM
	topography_dma_histories WITH(NOLOCK)
