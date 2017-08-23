CREATE PROCEDURE usp_property_histories_select_all
AS
SELECT
	*
FROM
	property_histories WITH(NOLOCK)
