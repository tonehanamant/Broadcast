CREATE PROCEDURE usp_employee_roles_delete
(
	@role_id		Int,
	@employee_id		Int)
AS
DELETE FROM
	employee_roles
WHERE
	role_id = @role_id
 AND
	employee_id = @employee_id
