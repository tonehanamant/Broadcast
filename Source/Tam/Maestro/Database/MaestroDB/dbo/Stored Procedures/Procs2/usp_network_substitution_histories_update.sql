CREATE PROCEDURE usp_network_substitution_histories_update
(
	@network_id		Int,
	@substitution_category_id		Int,
	@start_date		DateTime,
	@substitute_network_id		Int,
	@weight		Float,
	@end_date		DateTime
)
AS
UPDATE network_substitution_histories SET
	substitute_network_id = @substitute_network_id,
	weight = @weight,
	end_date = @end_date
WHERE
	network_id = @network_id AND
	substitution_category_id = @substitution_category_id AND
	start_date = @start_date
