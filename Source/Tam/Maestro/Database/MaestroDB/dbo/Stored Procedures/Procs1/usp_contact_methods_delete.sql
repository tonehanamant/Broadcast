CREATE PROCEDURE usp_contact_methods_delete
(
	@id Int
)
AS
DELETE FROM contact_methods WHERE id=@id
