-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to delete or potentionally disable a zone_zip_codes record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_codes_select]
	@zone_id INT,
	@zip_code CHAR(5)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zone_zip_codes].*
	FROM
		[dbo].[zone_zip_codes] WITH(NOLOCK)
	WHERE
		[zone_id]=@zone_id
		AND [zip_code]=@zip_code
END