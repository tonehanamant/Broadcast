CREATE PROCEDURE usp_cmw_contacts_delete
(
	@id Int
)
AS
DELETE FROM cmw_contacts WHERE id=@id
