-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/10/2014 11:51:28 AM
-- Description:	Auto-generated method to delete a single agency_code_mapset record.
-- =============================================
CREATE PROCEDURE usp_agency_code_mapset_delete
	@agency_code VARCHAR(63),
	@map_set VARCHAR(63)
AS
BEGIN
	DELETE FROM
		[dbo].[agency_code_mapset]
	WHERE
		[agency_code]=@agency_code
		AND [map_set]=@map_set
END

