
-- =============================================
-- Author:		<Nick Kheynis>
-- Create date: <2/7/14,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BS_DoesBroadcastAffidavitFileExist]
	@hash varchar(63)
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT COUNT(1) 
	FROM
		dbo.broadcast_affidavit_files baf (NOLOCK)
	WHERE
		baf.hash = @hash
	
END


