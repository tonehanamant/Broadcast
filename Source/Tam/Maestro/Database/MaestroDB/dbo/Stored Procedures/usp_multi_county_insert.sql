-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:56 AM
-- Description:	Auto-generated method to insert a multi_county record.
-- =============================================
CREATE PROCEDURE dbo.usp_multi_county_insert
	@zip_code CHAR(5),
	@state_fips CHAR(2),
	@state CHAR(2),
	@county_fips CHAR(3),
	@county VARCHAR(255)
AS
BEGIN
	INSERT INTO [zc].[multi_county]
	(
		[zip_code],
		[state_fips],
		[state],
		[county_fips],
		[county]
	)
	VALUES
	(
		@zip_code,
		@state_fips,
		@state,
		@county_fips,
		@county
	)
END