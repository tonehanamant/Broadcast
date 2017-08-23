CREATE PROCEDURE usp_network_substitution_histories_insert
(
	@network_id		Int,
	@substitution_category_id		Int,
	@start_date		DateTime,
	@substitute_network_id		Int,
	@weight		Float,
	@end_date		DateTime
)
AS
INSERT INTO network_substitution_histories
(
	network_id,
	substitution_category_id,
	start_date,
	substitute_network_id,
	weight,
	end_date
)
VALUES
(
	@network_id,
	@substitution_category_id,
	@start_date,
	@substitute_network_id,
	@weight,
	@end_date
)

