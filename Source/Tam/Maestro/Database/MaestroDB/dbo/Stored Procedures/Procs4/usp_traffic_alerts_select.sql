-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/23/2016 01:34:03 PM
-- Description:	Auto-generated method to select a single traffic_alerts record.
-- =============================================
CREATE PROCEDURE usp_traffic_alerts_select
	@id Int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[traffic_alerts].*
	FROM
		[dbo].[traffic_alerts] WITH(NOLOCK)
	WHERE
		[id]=@id
END
