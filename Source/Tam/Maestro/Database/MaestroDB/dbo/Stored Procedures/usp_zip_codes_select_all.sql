-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:58 AM
-- Description:	Auto-generated method to select all zip_codes records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_zip_codes_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[zip_codes].*
	FROM
		[zc].[zip_codes] WITH(NOLOCK)
END