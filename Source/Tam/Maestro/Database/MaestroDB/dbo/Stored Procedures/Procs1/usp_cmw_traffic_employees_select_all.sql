CREATE PROCEDURE usp_cmw_traffic_employees_select_all
AS
SELECT
	*
FROM
	cmw_traffic_employees WITH(NOLOCK)
