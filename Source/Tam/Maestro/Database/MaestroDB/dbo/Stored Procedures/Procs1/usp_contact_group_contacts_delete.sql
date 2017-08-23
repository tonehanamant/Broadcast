CREATE PROCEDURE usp_contact_group_contacts_delete
(
	@contact_id		Int,
	@contact_group_id		Int)
AS
DELETE FROM
	contact_group_contacts
WHERE
	contact_id = @contact_id
 AND
	contact_group_id = @contact_group_id
