CREATE PROCEDURE usp_employees_select_all
AS
SELECT
	*
FROM
	employees WITH(NOLOCK)
