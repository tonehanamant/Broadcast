CREATE PROCEDURE usp_proposal_contacts_select_all
AS
SELECT
	*
FROM
	proposal_contacts WITH(NOLOCK)
