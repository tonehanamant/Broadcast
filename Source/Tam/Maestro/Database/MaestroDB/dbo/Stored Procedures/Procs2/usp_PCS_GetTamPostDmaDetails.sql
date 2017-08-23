-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/18/2010
-- Description:	Retrieves all TamPostDmaDetail records for the given list of TamPostProposal Ids.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostDmaDetails]
	@tam_post_proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		tpdd.*
	FROM
		tam_post_dma_details tpdd (NOLOCK)
	WHERE
		tpdd.tam_post_proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_proposal_ids)
		)
END
