CREATE PROCEDURE [dbo].[usp_msa_post_daypart_details_update]
(
	@media_month_id		Int,
	@tam_post_id		Int,
	@msa_post_id		Int,
	@audience_id		Int,
	@network_id		Int,
	@daypart_id		Int,
	@file_type_id		Int,
	@delivery		Float
)
AS
UPDATE dbo.msa_post_daypart_details SET
	file_type_id = @file_type_id,
	delivery = @delivery
WHERE
	media_month_id = @media_month_id AND
	tam_post_id = @tam_post_id AND
	msa_post_id = @msa_post_id AND
	audience_id = @audience_id AND
	network_id = @network_id AND
	daypart_id = @daypart_id
