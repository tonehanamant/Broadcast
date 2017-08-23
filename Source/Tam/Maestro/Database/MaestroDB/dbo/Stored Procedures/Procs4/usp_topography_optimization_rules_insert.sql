CREATE PROCEDURE usp_topography_optimization_rules_insert
(
	@id		Int		OUTPUT,
	@topography_id		Int,
	@target_topography_id		Int,
	@optimization_ratio		Float,
	@rule_set		VarChar(63)
)
AS
INSERT INTO topography_optimization_rules
(
	topography_id,
	target_topography_id,
	optimization_ratio,
	rule_set
)
VALUES
(
	@topography_id,
	@target_topography_id,
	@optimization_ratio,
	@rule_set
)

SELECT
	@id = SCOPE_IDENTITY()

