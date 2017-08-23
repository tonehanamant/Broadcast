

/****** Object:  StoredProcedure [dbo].[usp_broadcast_posts_select]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_posts_select
(
	@broadcast_affidavit_file_id		Int
)
AS
	SELECT
		*
	FROM
		dbo.broadcast_posts WITH(NOLOCK)
	WHERE
		broadcast_affidavit_file_id=@broadcast_affidavit_file_id
