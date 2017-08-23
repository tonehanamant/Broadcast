

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavit_files_delete]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavit_files_delete
(
	@id Int
)
AS
	DELETE FROM dbo.broadcast_affidavit_files WHERE id=@id
