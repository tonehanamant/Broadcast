-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/18/2014 10:11:58 AM
-- Description:	Auto-generated method to insert a msa_posts record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_msa_posts_insert]
	@media_month_id INT,
	@tam_post_id INT,
	@id INT OUTPUT,
	@audience_id INT,
	@spot_length_id INT,
	@file_type_id INT,
	@file_name VARCHAR(255),
	@agency VARCHAR(255),
	@advertiser VARCHAR(255),
	@product VARCHAR(255),
	@source VARCHAR(255),
	@total_delivered FLOAT,
	@date_started DATETIME,
	@date_completed DATETIME
AS
BEGIN
	INSERT INTO [dbo].[msa_posts]
	(
		[media_month_id],
		[tam_post_id],
		[audience_id],
		[spot_length_id],
		[file_type_id],
		[file_name],
		[agency],
		[advertiser],
		[product],
		[source],
		[total_delivered],
		[date_started],
		[date_completed]
	)
	VALUES
	(
		@media_month_id,
		@tam_post_id,
		@audience_id,
		@spot_length_id,
		@file_type_id,
		@file_name,
		@agency,
		@advertiser,
		@product,
		@source,
		@total_delivered,
		@date_started,
		@date_completed
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
