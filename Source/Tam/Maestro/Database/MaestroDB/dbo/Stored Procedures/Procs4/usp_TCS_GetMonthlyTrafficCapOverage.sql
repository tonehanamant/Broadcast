-- =============================================
-- Author:		Steve/Joe
-- Create date: 3/18/2014
-- Description:	<Description,,>
-- =============================================
-- usp_TCS_GetMonthlyTrafficCapOverage 7233
CREATE PROCEDURE [dbo].[usp_TCS_GetMonthlyTrafficCapOverage]
	@traffic_id INT
AS
BEGIN
	SELECT * FROM udf_GetMonthlyTrafficCapOverage(@traffic_id);
END
