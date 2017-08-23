CREATE PROCEDURE usp_properties_select_all
AS
SELECT
	*
FROM
	properties WITH(NOLOCK)
