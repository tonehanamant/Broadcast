-- =============================================
-- Author: MNorris
-- Create date: 04/14/16
-- Description:	Gets a list of zip_codes.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetAllZipCodes]
	-- Add the parameters for the stored procedure here
	@state varchar(63),
	@county varchar(255) = null
AS
BEGIN
	SET NOCOUNT ON;
		select distinct zip_code, [state], county from zc.zip_codes
		order by zip_code, state, county asc
END