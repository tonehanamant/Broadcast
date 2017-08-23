CREATE PROCEDURE usp_dma_histories_select_all
AS
SELECT
	*
FROM
	dma_histories WITH(NOLOCK)
