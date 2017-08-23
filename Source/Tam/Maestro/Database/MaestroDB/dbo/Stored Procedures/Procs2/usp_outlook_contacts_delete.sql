CREATE PROCEDURE usp_outlook_contacts_delete
(
	@id Int
)
AS
DELETE FROM outlook_contacts WHERE id=@id
