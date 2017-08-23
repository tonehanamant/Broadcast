-- =============================================
-- Author:		CRUD Creator
-- Create date: 11/10/2014 11:51:27 AM
-- Description:	Auto-generated method to insert a agency_code_mapset record.
-- =============================================
CREATE PROCEDURE usp_agency_code_mapset_insert
	@agency_code VARCHAR(63),
	@map_set VARCHAR(63),
	@map_value VARCHAR(255)
AS
BEGIN
	INSERT INTO [dbo].[agency_code_mapset]
	(
		[agency_code],
		[map_set],
		[map_value]
	)
	VALUES
	(
		@agency_code,
		@map_set,
		@map_value
	)
END

