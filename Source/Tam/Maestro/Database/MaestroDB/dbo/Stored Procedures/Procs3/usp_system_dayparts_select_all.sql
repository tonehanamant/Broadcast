CREATE PROCEDURE usp_system_dayparts_select_all
AS
SELECT
	*
FROM
	system_dayparts WITH(NOLOCK)
