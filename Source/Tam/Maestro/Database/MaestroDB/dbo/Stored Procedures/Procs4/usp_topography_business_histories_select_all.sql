CREATE PROCEDURE usp_topography_business_histories_select_all
AS
SELECT
	*
FROM
	topography_business_histories WITH(NOLOCK)
