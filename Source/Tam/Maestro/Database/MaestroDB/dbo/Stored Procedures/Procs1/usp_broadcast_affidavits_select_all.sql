

/****** Object:  StoredProcedure [dbo].[usp_broadcast_affidavits_select_all]    Script Date: 04/03/2014 08:16:55 ******/
CREATE PROCEDURE usp_broadcast_affidavits_select_all
AS
	SELECT
		*
	FROM
		dbo.broadcast_affidavits WITH(NOLOCK)
