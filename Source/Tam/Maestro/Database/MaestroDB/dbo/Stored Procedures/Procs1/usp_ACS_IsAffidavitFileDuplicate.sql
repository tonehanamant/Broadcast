-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_IsAffidavitFileDuplicate]
	@hash VARCHAR(63)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		COUNT(*) 
	FROM 
		affidavit_files (NOLOCK) 
	WHERE 
		hash=@hash
END
