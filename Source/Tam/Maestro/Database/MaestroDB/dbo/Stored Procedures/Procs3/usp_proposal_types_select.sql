CREATE PROCEDURE usp_proposal_types_select
(
	@id Int
)
AS
SELECT
	*
FROM
	proposal_types WITH(NOLOCK)
WHERE
	id = @id
