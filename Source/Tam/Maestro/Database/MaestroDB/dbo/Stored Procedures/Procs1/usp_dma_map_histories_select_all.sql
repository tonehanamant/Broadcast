CREATE PROCEDURE usp_dma_map_histories_select_all
AS
SELECT
	*
FROM
	dma_map_histories WITH(NOLOCK)
