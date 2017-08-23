CREATE PROCEDURE usp_proposal_contacts_select
(
	@proposal_id		Int,
	@contact_id		Int,
	@date_created		DateTime
)
AS
SELECT
	*
FROM
	proposal_contacts WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	contact_id=@contact_id
	AND
	date_created=@date_created

