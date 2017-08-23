CREATE PROCEDURE usp_tam_post_network_details_delete
(
	@id Int
)
AS
DELETE FROM tam_post_network_details WHERE id=@id
