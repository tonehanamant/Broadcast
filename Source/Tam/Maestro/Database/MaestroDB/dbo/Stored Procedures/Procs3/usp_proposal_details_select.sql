CREATE PROCEDURE usp_proposal_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	proposal_details WITH(NOLOCK)
WHERE
	id = @id
