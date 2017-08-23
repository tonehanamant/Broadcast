-- =============================================
-- Author:		Steve/Joe
-- Create date: 3/18/2014
-- Description:	<Description,,>
-- =============================================
-- usp_TCS_GetTrafficCapOverage 10187
CREATE PROCEDURE usp_TCS_GetTrafficCapOverage
	@traffic_id INT
AS
BEGIN
	EXEC dbo.usp_TCS_GetMonthlyTrafficCapOverage @traffic_id;
	EXEC dbo.usp_TCS_GetCumulativeTrafficCapOverage @traffic_id;
END
