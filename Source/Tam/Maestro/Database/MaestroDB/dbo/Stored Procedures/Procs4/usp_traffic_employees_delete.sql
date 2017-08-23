CREATE PROCEDURE usp_traffic_employees_delete
(
	@traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime)
AS
DELETE FROM
	traffic_employees
WHERE
	traffic_id = @traffic_id
 AND
	employee_id = @employee_id
 AND
	effective_date = @effective_date
