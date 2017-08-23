-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/18/2010
-- Description:	Retrieves all TamPostSystemDetailAudience records for the given list of TamPostProposal Ids.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostSystemDetailAudiences]
	@tam_post_proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT DISTINCT
		tpsda.*
	FROM
		tam_post_system_detail_audiences tpsda (NOLOCK)
		JOIN tam_post_system_details tpsd (NOLOCK) ON tpsd.id=tpsda.tam_post_system_detail_id
	WHERE
		tpsd.tam_post_proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_proposal_ids)
		)
END
