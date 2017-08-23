-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:32 AM
-- Description:	Auto-generated method to update a ratings record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ratings_update]
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
	UPDATE
		[dbo].[ratings]
	SET
		[audience_usage]=@audience_usage,
		[tv_usage]=@tv_usage
	WHERE
		[rating_category_id]=@rating_category_id
		AND [base_media_month_id]=@base_media_month_id
		AND [forecast_media_month_id]=@forecast_media_month_id
		AND [nielsen_network_id]=@nielsen_network_id
		AND [audience_id]=@audience_id
		AND [daypart_id]=@daypart_id
END
