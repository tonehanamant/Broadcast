
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:09:00 PM
-- Description:	Auto-generated method to select all traffic_detail_topographies records.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_topographies_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_detail_topographies].*
	FROM
		[dbo].[traffic_detail_topographies] WITH(NOLOCK)
END
