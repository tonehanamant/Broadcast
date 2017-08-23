CREATE PROCEDURE usp_network_substitution_histories_select
(
	@network_id		Int,
	@substitution_category_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	network_substitution_histories WITH(NOLOCK)
WHERE
	network_id=@network_id
	AND
	substitution_category_id=@substitution_category_id
	AND
	start_date=@start_date

