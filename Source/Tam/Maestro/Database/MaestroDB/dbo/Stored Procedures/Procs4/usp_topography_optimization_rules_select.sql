CREATE PROCEDURE usp_topography_optimization_rules_select
(
	@id Int
)
AS
SELECT
	*
FROM
	topography_optimization_rules WITH(NOLOCK)
WHERE
	id = @id
