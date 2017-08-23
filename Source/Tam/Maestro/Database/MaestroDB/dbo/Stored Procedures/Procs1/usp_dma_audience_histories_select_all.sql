CREATE PROCEDURE usp_dma_audience_histories_select_all
AS
SELECT
	*
FROM
	dma_audience_histories WITH(NOLOCK)
