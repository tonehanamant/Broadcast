CREATE PROCEDURE usp_zone_business_histories_select_all
AS
SELECT
	*
FROM
	zone_business_histories WITH(NOLOCK)
