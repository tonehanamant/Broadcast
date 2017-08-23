-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/18/2010
-- Description:	Retrieves all TamPostSystemDetail records for the given list of TamPostProposal Ids.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostSystemDetails]
	@tam_post_proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		tpsd.*
	FROM
		tam_post_system_details tpsd (NOLOCK)
	WHERE
		tpsd.tam_post_proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_proposal_ids)
		)
END
