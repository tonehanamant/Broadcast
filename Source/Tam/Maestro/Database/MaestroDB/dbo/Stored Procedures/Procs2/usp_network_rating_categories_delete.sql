CREATE PROCEDURE usp_network_rating_categories_delete
(
	@id Int
)
AS
DELETE FROM network_rating_categories WHERE id=@id
