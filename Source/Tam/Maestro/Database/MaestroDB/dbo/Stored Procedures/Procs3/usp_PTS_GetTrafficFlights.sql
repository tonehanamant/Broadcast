
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PTS_GetTrafficFlights]
	@traffic_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

select 
		traffic_flights.start_date, 
		traffic_flights.end_date, 
		traffic_flights.selected 
from 
	traffic_flights (NOLOCK) 
where 
	traffic_id = @traffic_id
END

