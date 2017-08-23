-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/23/2016 01:34:03 PM
-- Description:	Auto-generated method to select all traffic_alerts records.
-- =============================================
CREATE PROCEDURE usp_traffic_alerts_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_alerts].*
	FROM
		[dbo].[traffic_alerts] WITH(NOLOCK)
END
