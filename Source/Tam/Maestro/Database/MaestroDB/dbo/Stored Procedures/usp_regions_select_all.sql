-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/13/2016 10:15:55 AM
-- Description:	Auto-generated method to select all regions records.
-- =============================================
create PROCEDURE dbo.usp_regions_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[regions].*
	FROM
		[dbo].[regions] WITH(NOLOCK)
END