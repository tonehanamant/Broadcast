CREATE PROCEDURE usp_traffic_categories_delete
(
	@id Int
)
AS
DELETE FROM traffic_categories WHERE id=@id
