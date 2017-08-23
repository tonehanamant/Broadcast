-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to update a zone_zip_code_histories record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_code_histories_update]
	@zone_id INT,
	@zip_code CHAR(5),
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[zone_zip_code_histories]
	SET
		[end_date]=@end_date
	WHERE
		[zone_id]=@zone_id
		AND [zip_code]=@zip_code
		AND [start_date]=@start_date
END