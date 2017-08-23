-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/10/2014 11:51:27 AM
-- Description:	Auto-generated method to update a agency_code_mapset record.
-- =============================================
CREATE PROCEDURE usp_agency_code_mapset_update
	@agency_code VARCHAR(63),
	@map_set VARCHAR(63),
	@map_value VARCHAR(255)
AS
BEGIN
	UPDATE
		[dbo].[agency_code_mapset]
	SET
		[map_value]=@map_value
	WHERE
		[agency_code]=@agency_code
		AND [map_set]=@map_set
END
