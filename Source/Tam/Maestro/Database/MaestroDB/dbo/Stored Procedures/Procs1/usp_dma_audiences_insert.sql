-- =============================================
-- Author:		CRUD Creator
-- Create date: 12/10/2013 03:37:46 PM
-- Description:	Auto-generated method to insert a dma_audiences record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_dma_audiences_insert]
	@rating_category_group_id TINYINT,
	@media_month_id INT,
	@dma_id INT,
	@audience_id INT,
	@universe INT
AS
BEGIN
	INSERT INTO [dbo].[dma_audiences]
	(
		[rating_category_group_id],
		[media_month_id],
		[dma_id],
		[audience_id],
		[universe]
	)
	VALUES
	(
		@rating_category_group_id,
		@media_month_id,
		@dma_id,
		@audience_id,
		@universe
	)
END
