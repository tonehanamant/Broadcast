-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to delete a single zone_zip_codes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_code_delete]
	@zone_id INT,
	@zip_code CHAR(5)
AS
BEGIN
	DELETE FROM
		[dbo].[zone_zip_codes]
	WHERE
		[zone_id]=@zone_id
		AND [zip_code]=@zip_code
END