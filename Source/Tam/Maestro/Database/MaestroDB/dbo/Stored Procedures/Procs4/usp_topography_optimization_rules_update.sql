CREATE PROCEDURE usp_topography_optimization_rules_update
(
	@id		Int,
	@topography_id		Int,
	@target_topography_id		Int,
	@optimization_ratio		Float,
	@rule_set		VarChar(63)
)
AS
UPDATE topography_optimization_rules SET
	topography_id = @topography_id,
	target_topography_id = @target_topography_id,
	optimization_ratio = @optimization_ratio,
	rule_set = @rule_set
WHERE
	id = @id

