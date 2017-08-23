CREATE PROCEDURE usp_employee_roles_insert
(
	@role_id		Int,
	@employee_id		Int
)
AS
INSERT INTO employee_roles
(
	role_id,
	employee_id
)
VALUES
(
	@role_id,
	@employee_id
)

