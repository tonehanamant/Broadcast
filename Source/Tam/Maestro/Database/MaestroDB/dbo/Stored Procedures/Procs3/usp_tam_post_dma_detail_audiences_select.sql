CREATE PROCEDURE usp_tam_post_dma_detail_audiences_select
(
	@tam_post_dma_detail_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	tam_post_dma_detail_audiences WITH(NOLOCK)
WHERE
	tam_post_dma_detail_id=@tam_post_dma_detail_id
	AND
	audience_id=@audience_id

