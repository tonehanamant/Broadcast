

 

 

CREATE Procedure [dbo].[usp_TCS_GetLastEmployeeEdit]

      (

            @id Int 

      )

AS

Select top 1 employee_id, effective_date from traffic_employees (NOLOCK) where traffic_id =@id 

order by effective_date desc

