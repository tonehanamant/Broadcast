CREATE PROCEDURE usp_proposal_contacts_insert
(
	@proposal_id		Int,
	@contact_id		Int,
	@date_created		DateTime
)
AS
INSERT INTO proposal_contacts
(
	proposal_id,
	contact_id,
	date_created
)
VALUES
(
	@proposal_id,
	@contact_id,
	@date_created
)

