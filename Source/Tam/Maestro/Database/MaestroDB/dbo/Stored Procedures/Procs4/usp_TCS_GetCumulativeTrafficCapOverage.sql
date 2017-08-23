-- =============================================
-- Author:		Steve/Joe
-- Create date: 3/18/2014
-- Description:	<Description,,>
-- =============================================
-- usp_TCS_GetCumulativeTrafficCapOverage 7233
CREATE PROCEDURE usp_TCS_GetCumulativeTrafficCapOverage
	@traffic_id INT
AS
BEGIN
	SELECT * FROM dbo.udf_GetCumulativeTrafficCapOverage(@traffic_id);
END
