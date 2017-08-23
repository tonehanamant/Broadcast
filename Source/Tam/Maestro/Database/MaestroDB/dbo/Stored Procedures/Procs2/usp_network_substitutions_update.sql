CREATE PROCEDURE usp_network_substitutions_update
(
	@network_id		Int,
	@substitution_category_id		Int,
	@substitute_network_id		Int,
	@weight		Float,
	@effective_date		DateTime
)
AS
UPDATE network_substitutions SET
	substitute_network_id = @substitute_network_id,
	weight = @weight,
	effective_date = @effective_date
WHERE
	network_id = @network_id AND
	substitution_category_id = @substitution_category_id
