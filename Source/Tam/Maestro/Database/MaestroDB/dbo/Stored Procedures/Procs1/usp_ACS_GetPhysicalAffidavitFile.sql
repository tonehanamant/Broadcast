-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ACS_GetPhysicalAffidavitFile
	@hash CHAR(59)
AS
BEGIN
    SELECT
		physical_file
	FROM
		affidavit_files (NOLOCK)
	WHERE
		hash=@hash
END
