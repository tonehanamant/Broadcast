CREATE PROCEDURE usp_contacts_delete
(
	@id Int
)
AS
DELETE FROM contacts WHERE id=@id
