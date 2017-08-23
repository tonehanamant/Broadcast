

/****** Object:  StoredProcedure [dbo].[usp_broadcast_posts_update]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_posts_update
(
	@broadcast_affidavit_file_id		Int,
	@status_code		TinyInt,
	@date_queued		DateTime,
	@date_started		DateTime,
	@date_completed		DateTime
)
AS
	UPDATE dbo.broadcast_posts SET
		status_code = @status_code,
		date_queued = @date_queued,
		date_started = @date_started,
		date_completed = @date_completed
	WHERE
		broadcast_affidavit_file_id = @broadcast_affidavit_file_id
