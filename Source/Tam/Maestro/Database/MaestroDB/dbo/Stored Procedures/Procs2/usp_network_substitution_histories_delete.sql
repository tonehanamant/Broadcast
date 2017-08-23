CREATE PROCEDURE usp_network_substitution_histories_delete
(
	@network_id		Int,
	@substitution_category_id		Int,
	@start_date		DateTime)
AS
DELETE FROM
	network_substitution_histories
WHERE
	network_id = @network_id
 AND
	substitution_category_id = @substitution_category_id
 AND
	start_date = @start_date
