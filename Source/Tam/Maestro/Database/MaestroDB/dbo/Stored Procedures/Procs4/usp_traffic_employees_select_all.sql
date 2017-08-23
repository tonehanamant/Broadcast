CREATE PROCEDURE usp_traffic_employees_select_all
AS
SELECT
	*
FROM
	traffic_employees WITH(NOLOCK)
