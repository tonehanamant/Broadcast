
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalsRevisions]
(
	@proposal_id Int
)
AS

DECLARE @original_proposal_id AS INT
SET @original_proposal_id = (SELECT original_proposal_id FROM proposals (NOLOCK) WHERE id=@proposal_id)

IF @original_proposal_id IS NULL
	BEGIN
		SET @original_proposal_id = @proposal_id
	END

SELECT
	*
FROM
	proposals (NOLOCK)
WHERE
	(id=@original_proposal_id OR original_proposal_id=@original_proposal_id)
	AND id<>@proposal_id

