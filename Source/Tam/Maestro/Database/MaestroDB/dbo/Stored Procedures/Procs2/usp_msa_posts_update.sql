CREATE PROCEDURE [dbo].[usp_msa_posts_update]
(
	@media_month_id		Int,
	@tam_post_id		Int,
	@id		Int,
	@audience_id		Int,
	@spot_length_id		Int,
	@file_type_id		Int,
	@file_name		VarChar(255),
	@agency		VarChar(255),
	@advertiser		VarChar(255),
	@product		VarChar(255),
	@source		VarChar(255),
	@total_delivered		Float,
	@date_started		DateTime,
	@date_completed		DateTime
)
AS
UPDATE dbo.msa_posts SET
	media_month_id = @media_month_id,
	tam_post_id = @tam_post_id,
	audience_id = @audience_id,
	spot_length_id = @spot_length_id,
	file_type_id = @file_type_id,
	file_name = @file_name,
	agency = @agency,
	advertiser = @advertiser,
	product = @product,
	source = @source,
	total_delivered = @total_delivered,
	date_started = @date_started,
	date_completed = @date_completed
WHERE
	id = @id
