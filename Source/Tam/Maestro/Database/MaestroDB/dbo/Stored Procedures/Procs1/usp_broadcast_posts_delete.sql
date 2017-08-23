

/****** Object:  StoredProcedure [dbo].[usp_broadcast_posts_delete]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_posts_delete
(
	@broadcast_affidavit_file_id		Int)
AS
	DELETE FROM
		dbo.broadcast_posts
	WHERE
		broadcast_affidavit_file_id = @broadcast_affidavit_file_id
