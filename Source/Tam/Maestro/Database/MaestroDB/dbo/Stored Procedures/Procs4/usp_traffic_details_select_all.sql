
-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/06/2015 12:15:41 AM
-- Description:	Auto-generated method to select all traffic_details records.
-- =============================================
CREATE PROCEDURE usp_traffic_details_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_details].*
	FROM
		[dbo].[traffic_details] WITH(NOLOCK)
END
