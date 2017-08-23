CREATE PROCEDURE usp_activity_types_select_all
AS
SELECT
	*
FROM
	activity_types WITH(NOLOCK)
