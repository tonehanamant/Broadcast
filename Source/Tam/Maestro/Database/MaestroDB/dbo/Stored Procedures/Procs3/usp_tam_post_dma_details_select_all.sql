CREATE PROCEDURE usp_tam_post_dma_details_select_all
AS
SELECT
	*
FROM
	tam_post_dma_details WITH(NOLOCK)
