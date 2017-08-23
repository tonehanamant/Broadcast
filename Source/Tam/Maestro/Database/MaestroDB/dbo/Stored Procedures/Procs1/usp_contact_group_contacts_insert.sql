CREATE PROCEDURE usp_contact_group_contacts_insert
(
	@contact_id		Int,
	@contact_group_id		Int
)
AS
INSERT INTO contact_group_contacts
(
	contact_id,
	contact_group_id
)
VALUES
(
	@contact_id,
	@contact_group_id
)

