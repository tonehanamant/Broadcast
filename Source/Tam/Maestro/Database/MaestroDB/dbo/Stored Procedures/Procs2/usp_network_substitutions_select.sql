CREATE PROCEDURE usp_network_substitutions_select
(
	@network_id		Int,
	@substitution_category_id		Int
)
AS
SELECT
	*
FROM
	network_substitutions WITH(NOLOCK)
WHERE
	network_id=@network_id
	AND
	substitution_category_id=@substitution_category_id

