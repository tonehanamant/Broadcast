CREATE PROCEDURE usp_coverage_universes_select
(
	@id Int
)
AS
SELECT
	*
FROM
	coverage_universes WITH(NOLOCK)
WHERE
	id = @id
