-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:56 AM
-- Description:	Auto-generated method to select all multi_county records.
-- =============================================
CREATE PROCEDURE dbo.usp_multi_county_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[multi_county].*
	FROM
		[zc].[multi_county] WITH(NOLOCK)
END