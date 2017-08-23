
-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/18/2015 02:08:59 PM
-- Description:	Auto-generated method to select all traffic_detail_audiences records.
-- =============================================
CREATE PROCEDURE usp_traffic_detail_audiences_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_detail_audiences].*
	FROM
		[dbo].[traffic_detail_audiences] WITH(NOLOCK)
END
