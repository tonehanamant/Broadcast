-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:33 AM
-- Description:	Auto-generated method to insert a universes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_universes_insert]
	@rating_category_id INT,
	@base_media_month_id INT,
	@forecast_media_month_id INT,
	@nielsen_network_id INT,
	@audience_id INT,
	@universe FLOAT
AS
BEGIN
	INSERT INTO [dbo].[universes]
	(
		[rating_category_id],
		[base_media_month_id],
		[forecast_media_month_id],
		[nielsen_network_id],
		[audience_id],
		[universe]
	)
	VALUES
	(
		@rating_category_id,
		@base_media_month_id,
		@forecast_media_month_id,
		@nielsen_network_id,
		@audience_id,
		@universe
	)
END
