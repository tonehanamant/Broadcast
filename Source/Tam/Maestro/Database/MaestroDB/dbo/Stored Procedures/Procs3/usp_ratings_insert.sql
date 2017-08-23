-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:32 AM
-- Description:	Auto-generated method to insert a ratings record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ratings_insert]
	@rating_category_id INT,
	@base_media_month_id INT,
	@forecast_media_month_id INT,
	@nielsen_network_id INT,
	@audience_id INT,
	@daypart_id INT,
	@audience_usage FLOAT,
	@tv_usage FLOAT
AS
BEGIN
	INSERT INTO [dbo].[ratings]
	(
		[rating_category_id],
		[base_media_month_id],
		[forecast_media_month_id],
		[nielsen_network_id],
		[audience_id],
		[daypart_id],
		[audience_usage],
		[tv_usage]
	)
	VALUES
	(
		@rating_category_id,
		@base_media_month_id,
		@forecast_media_month_id,
		@nielsen_network_id,
		@audience_id,
		@daypart_id,
		@audience_usage,
		@tv_usage
	)
END
