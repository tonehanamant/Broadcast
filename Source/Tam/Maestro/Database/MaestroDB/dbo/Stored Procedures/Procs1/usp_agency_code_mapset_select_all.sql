-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/10/2014 11:51:27 AM
-- Description:	Auto-generated method to select all agency_code_mapset records.
-- =============================================
CREATE PROCEDURE usp_agency_code_mapset_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[agency_code_mapset].*
	FROM
		[dbo].[agency_code_mapset] WITH(NOLOCK)
END

