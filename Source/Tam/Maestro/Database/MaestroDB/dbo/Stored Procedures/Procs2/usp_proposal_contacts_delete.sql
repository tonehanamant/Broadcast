CREATE PROCEDURE usp_proposal_contacts_delete
(
	@proposal_id		Int,
	@contact_id		Int,
	@date_created		DateTime)
AS
DELETE FROM
	proposal_contacts
WHERE
	proposal_id = @proposal_id
 AND
	contact_id = @contact_id
 AND
	date_created = @date_created
