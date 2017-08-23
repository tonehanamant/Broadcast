

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavits_delete]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavits_delete
(
	@id BigInt
)
AS
	DELETE FROM dbo.broadcast_affidavits WHERE id=@id
