CREATE PROCEDURE usp_network_substitutions_insert
(
	@network_id		Int,
	@substitution_category_id		Int,
	@substitute_network_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
INSERT INTO network_substitutions
(
	network_id,
	substitution_category_id,
	substitute_network_id,
	weight,
	effective_date
)
VALUES
(
	@network_id,
	@substitution_category_id,
	@substitute_network_id,
	@weight,
	@effective_date
)

