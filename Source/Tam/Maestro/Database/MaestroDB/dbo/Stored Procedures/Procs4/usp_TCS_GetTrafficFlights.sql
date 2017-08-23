


CREATE Procedure [dbo].[usp_TCS_GetTrafficFlights]
	(
		@id Int
	)
AS
SELECT 
	traffic_id, 
	start_date, 
	end_date, 
	selected
from 
	traffic_flights (NOLOCK) 
where 
	traffic_id = @id
order by 
	start_date



