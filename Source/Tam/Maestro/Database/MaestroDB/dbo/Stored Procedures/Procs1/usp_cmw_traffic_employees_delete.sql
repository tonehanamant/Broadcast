CREATE PROCEDURE usp_cmw_traffic_employees_delete
(
	@cmw_traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime)
AS
DELETE FROM
	cmw_traffic_employees
WHERE
	cmw_traffic_id = @cmw_traffic_id
 AND
	employee_id = @employee_id
 AND
	effective_date = @effective_date
