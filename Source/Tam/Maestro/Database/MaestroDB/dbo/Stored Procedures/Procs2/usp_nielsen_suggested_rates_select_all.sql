CREATE PROCEDURE usp_nielsen_suggested_rates_select_all
AS
SELECT
	*
FROM
	nielsen_suggested_rates WITH(NOLOCK)
