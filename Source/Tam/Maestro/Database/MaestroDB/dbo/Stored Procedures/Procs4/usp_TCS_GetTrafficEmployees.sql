
CREATE Procedure [dbo].[usp_TCS_GetTrafficEmployees]
	(
		@id Int
	)
AS
SELECT traffic_id, employee_id, effective_date
from 
traffic_employees (NOLOCK) where traffic_id = @id

