CREATE PROCEDURE usp_dayparts_select_all
AS
SELECT
	*
FROM
	dayparts WITH(NOLOCK)
