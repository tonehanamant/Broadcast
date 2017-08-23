CREATE PROCEDURE usp_employee_roles_select_all
AS
SELECT
	*
FROM
	employee_roles WITH(NOLOCK)
