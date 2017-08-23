CREATE PROCEDURE usp_tam_post_system_details_delete
(
	@id Int
)
AS
DELETE FROM tam_post_system_details WHERE id=@id
