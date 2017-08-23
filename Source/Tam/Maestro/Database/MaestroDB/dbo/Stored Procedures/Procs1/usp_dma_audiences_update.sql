CREATE PROCEDURE [dbo].[usp_dma_audiences_update]
(
	@rating_category_group_id		TinyInt,
	@media_month_id		Int,
	@dma_id		Int,
	@audience_id		Int,
	@universe		Int
)
AS
BEGIN
	UPDATE dbo.dma_audiences SET
		universe = @universe
	WHERE
		rating_category_group_id = @rating_category_group_id AND
		media_month_id = @media_month_id AND
		dma_id = @dma_id AND
		audience_id = @audience_id
END
