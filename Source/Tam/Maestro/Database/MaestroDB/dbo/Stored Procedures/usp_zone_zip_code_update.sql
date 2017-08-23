-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to update a zone_zip_codes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_code_update]
	@zone_id INT,
	@zip_code CHAR(5),
	@effective_date DATETIME
AS
BEGIN
	UPDATE
		[dbo].[zone_zip_codes]
	SET
		[effective_date]=@effective_date
	WHERE
		[zone_id]=@zone_id
		AND [zip_code]=@zip_code
END