-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to insert a zone_zip_code_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_code_histories_insert]
	@zone_id INT,
	@zip_code CHAR(5),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	INSERT INTO [dbo].[zone_zip_code_histories]
	(
		[zone_id],
		[zip_code],
		[start_date],
		[end_date]
	)
	VALUES
	(
		@zone_id,
		@zip_code,
		@start_date,
		@end_date
	)
END