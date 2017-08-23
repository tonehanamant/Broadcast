CREATE PROCEDURE usp_zone_dma_histories_select_all
AS
SELECT
	*
FROM
	zone_dma_histories WITH(NOLOCK)
