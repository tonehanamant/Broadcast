-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/18/2010
-- Description:	Retrieves all TamPostDmaDetailAudience records for the given list of TamPostProposal Ids.
-- =============================================
-- usp_PCS_GetTamPostDmaDetailAudiences '924'
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostDmaDetailAudiences]
	@tam_post_proposal_ids VARCHAR(MAX)
AS
BEGIN
    SELECT DISTINCT
		tpdda.*
	FROM
		tam_post_dma_detail_audiences tpdda (NOLOCK)
		JOIN tam_post_dma_details tpdd (NOLOCK) ON tpdd.id=tpdda.tam_post_dma_detail_id
	WHERE
		tpdd.tam_post_proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_proposal_ids)
		)
END
