CREATE PROCEDURE usp_topography_optimization_rules_delete
(
	@id Int
)
AS
DELETE FROM topography_optimization_rules WHERE id=@id
