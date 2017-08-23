CREATE PROCEDURE [dbo].[usp_broadcast_post_details_delete]
(
	@media_month_id		Int,
	@broadcast_affidavit_file_id		Int,
	@broadcast_affidavit_id		BigInt,
	@audience_id		Int)
AS
	DELETE FROM
		dbo.broadcast_post_details
	WHERE
		media_month_id = @media_month_id
		AND broadcast_affidavit_file_id = @broadcast_affidavit_file_id
		AND broadcast_affidavit_id = @broadcast_affidavit_id
		AND audience_id = @audience_id
