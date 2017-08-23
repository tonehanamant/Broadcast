-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/18/2014 10:11:56 AM
-- Description:	Auto-generated method to insert a msa_post_weekly_details record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_post_weekly_details_insert]
	@media_month_id INT,
	@tam_post_id INT,
	@msa_post_id INT,
	@audience_id INT,
	@network_id INT,
	@media_week_id INT,
	@file_type_id INT,
	@delivery FLOAT
AS
BEGIN
	INSERT INTO [dbo].[msa_post_weekly_details]
	(
		[media_month_id],
		[tam_post_id],
		[msa_post_id],
		[audience_id],
		[network_id],
		[media_week_id],
		[file_type_id],
		[delivery]
	)
	VALUES
	(
		@media_month_id,
		@tam_post_id,
		@msa_post_id,
		@audience_id,
		@network_id,
		@media_week_id,
		@file_type_id,
		@delivery
	)
END
