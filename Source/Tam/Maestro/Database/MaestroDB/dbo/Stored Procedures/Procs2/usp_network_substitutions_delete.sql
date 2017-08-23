CREATE PROCEDURE usp_network_substitutions_delete
(
	@network_id		Int,
	@substitution_category_id		Int
)
AS
DELETE FROM network_substitutions WHERE network_id=@network_id AND substitution_category_id=@substitution_category_id
