CREATE PROCEDURE usp_activities_select_all
AS
SELECT
	*
FROM
	activities WITH(NOLOCK)
