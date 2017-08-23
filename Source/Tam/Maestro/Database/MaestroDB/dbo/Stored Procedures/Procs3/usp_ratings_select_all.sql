-- =============================================
-- Author:		CRUD Creator
-- Create date: 05/01/2014 11:23:32 AM
-- Description:	Auto-generated method to select all ratings records.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ratings_select_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[ratings].*
	FROM
		[dbo].[ratings] WITH(NOLOCK)
END
