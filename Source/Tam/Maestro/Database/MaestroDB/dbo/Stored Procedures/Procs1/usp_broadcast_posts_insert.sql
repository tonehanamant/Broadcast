

/****** Object:  StoredProcedure [dbo].[usp_broadcast_posts_insert]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_posts_insert
	@broadcast_affidavit_file_id INT,
	@status_code TINYINT,
	@date_queued DATETIME,
	@date_started DATETIME,
	@date_completed DATETIME
AS
BEGIN
	INSERT INTO [dbo].[broadcast_posts]
	(
		[broadcast_affidavit_file_id],
		[status_code],
		[date_queued],
		[date_started],
		[date_completed]
	)
	VALUES
	(
		@broadcast_affidavit_file_id,
		@status_code,
		@date_queued,
		@date_started,
		@date_completed
	)
END
