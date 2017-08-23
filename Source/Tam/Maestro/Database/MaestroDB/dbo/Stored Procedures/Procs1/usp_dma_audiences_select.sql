CREATE PROCEDURE [dbo].[usp_dma_audiences_select]
(
	@rating_category_group_id		TinyInt,
	@media_month_id		Int,
	@dma_id		Int,
	@audience_id		Int
)
AS
SELECT
	*
FROM
	dbo.dma_audiences WITH(NOLOCK)
WHERE
	rating_category_group_id=@rating_category_group_id
	AND
	media_month_id=@media_month_id
	AND
	dma_id=@dma_id
	AND
	audience_id=@audience_id
