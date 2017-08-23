-- =============================================
-- Author: MNorris
-- Create date: 04/14/16
-- Description:	Gets a list of zip_codes from a specified state.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetZipCodesForState]
	-- Add the parameters for the stored procedure here
	@state varchar(63),
	@county varchar(255) = null
AS
BEGIN
	SET NOCOUNT ON;
	if (@county <> '' AND @county IS NOT NULL)
	BEGIN
		select distinct zip_code, [state], county from zc.zip_codes
		where [state] = @state and county = @county
	END
	ELSE
	BEGIN 
		select distinct zip_code, [state], county from zc.zip_codes
		where [state] = @state
	END
END