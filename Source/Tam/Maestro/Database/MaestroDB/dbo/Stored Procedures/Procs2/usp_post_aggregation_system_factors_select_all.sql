CREATE PROCEDURE usp_post_aggregation_system_factors_select_all
AS
SELECT
	*
FROM
	post_aggregation_system_factors WITH(NOLOCK)
