CREATE PROCEDURE usp_employee_roles_select
(
	@role_id		Int,
	@employee_id		Int
)
AS
SELECT
	*
FROM
	employee_roles WITH(NOLOCK)
WHERE
	role_id=@role_id
	AND
	employee_id=@employee_id

