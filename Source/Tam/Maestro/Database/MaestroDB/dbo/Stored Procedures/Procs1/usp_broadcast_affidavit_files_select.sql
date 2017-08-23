/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavit_files_select]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavit_files_select
(
	@id Int
)
AS
	SELECT
		*
	FROM
		dbo.broadcast_affidavit_files WITH(NOLOCK)
	WHERE
		id = @id
