-- =============================================
-- Author: MNorris
-- Create date: 04/14/16
-- Description:	Gets a list of counties from a specified state.
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetCountiesForState]
	-- Add the parameters for the stored procedure here
	@state varchar(63)
AS
BEGIN
	SET NOCOUNT ON;

	select distinct county from zc.zip_codes
	where [state] = @state
END