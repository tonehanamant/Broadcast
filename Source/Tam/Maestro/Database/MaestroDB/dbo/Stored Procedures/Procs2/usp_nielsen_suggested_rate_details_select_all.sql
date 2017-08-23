CREATE PROCEDURE usp_nielsen_suggested_rate_details_select_all
AS
SELECT
	*
FROM
	nielsen_suggested_rate_details WITH(NOLOCK)
