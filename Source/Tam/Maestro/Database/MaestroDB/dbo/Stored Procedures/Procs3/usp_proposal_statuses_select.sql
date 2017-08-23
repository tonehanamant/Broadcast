CREATE PROCEDURE usp_proposal_statuses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	proposal_statuses WITH(NOLOCK)
WHERE
	id = @id
