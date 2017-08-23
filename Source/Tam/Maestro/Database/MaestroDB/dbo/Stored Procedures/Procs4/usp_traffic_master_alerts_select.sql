
-- =============================================
-- Author:		CRUD Creator
-- Create date: 07/22/2016 11:49:27 AM
-- Description:	Auto-generated method to select a single traffic_master_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_master_alerts_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_master_alerts].*
	FROM
		[dbo].[traffic_master_alerts] WITH(NOLOCK)
	WHERE
		[id]=@id
END
