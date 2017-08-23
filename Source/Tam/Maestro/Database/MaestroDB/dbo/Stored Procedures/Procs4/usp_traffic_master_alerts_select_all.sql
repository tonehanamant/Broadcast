
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/22/2016 11:49:27 AM
-- Description:	Auto-generated method to select all traffic_master_alerts records.
-- =============================================
CREATE PROCEDURE usp_traffic_master_alerts_select_all
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_master_alerts].*
	FROM
		[dbo].[traffic_master_alerts] WITH(NOLOCK)
END
