

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavits_select]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavits_select
(
	@id BigInt
)
AS
	SELECT
		*
	FROM
		dbo.broadcast_affidavits WITH(NOLOCK)
	WHERE
		id = @id
