CREATE PROCEDURE usp_contact_groups_delete
(
	@id Int
)
AS
DELETE FROM contact_groups WHERE id=@id
