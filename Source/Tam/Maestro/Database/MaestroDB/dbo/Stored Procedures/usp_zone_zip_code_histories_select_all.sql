-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/05/2016 12:51:42 PM
-- Description:	Auto-generated method to select all zone_zip_code_histories records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zone_zip_code_histories_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zone_zip_code_histories].*
	FROM
		[dbo].[zone_zip_code_histories] WITH(NOLOCK)
END