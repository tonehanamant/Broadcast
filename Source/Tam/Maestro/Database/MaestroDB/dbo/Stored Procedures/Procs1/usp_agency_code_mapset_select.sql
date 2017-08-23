-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/10/2014 11:51:27 AM
-- Description:	Auto-generated method to delete or potentionally disable a agency_code_mapset record.
-- =============================================
CREATE PROCEDURE usp_agency_code_mapset_select
	@agency_code VARCHAR(63),
	@map_set VARCHAR(63)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[agency_code_mapset].*
	FROM
		[dbo].[agency_code_mapset] WITH(NOLOCK)
	WHERE
		[agency_code]=@agency_code
		AND [map_set]=@map_set
END

