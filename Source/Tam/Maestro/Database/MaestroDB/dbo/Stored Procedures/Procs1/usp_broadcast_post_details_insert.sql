

/****** Object:  StoredProcedure [dbo].[usp_broadcast_post_details_insert]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_post_details_insert
	@media_month_id INT,
	@broadcast_affidavit_file_id INT,
	@broadcast_affidavit_id BIGINT,
	@audience_id INT,
	@delivery FLOAT
AS
BEGIN
	INSERT INTO [dbo].[broadcast_post_details]
	(
		[media_month_id],
		[broadcast_affidavit_file_id],
		[broadcast_affidavit_id],
		[audience_id],
		[delivery]
	)
	VALUES
	(
		@media_month_id,
		@broadcast_affidavit_file_id,
		@broadcast_affidavit_id,
		@audience_id,
		@delivery
	)
END
