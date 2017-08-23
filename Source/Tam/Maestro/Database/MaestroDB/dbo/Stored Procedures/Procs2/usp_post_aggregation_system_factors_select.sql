CREATE PROCEDURE usp_post_aggregation_system_factors_select
(
	@id Int
)
AS
SELECT
	*
FROM
	post_aggregation_system_factors WITH(NOLOCK)
WHERE
	id = @id
