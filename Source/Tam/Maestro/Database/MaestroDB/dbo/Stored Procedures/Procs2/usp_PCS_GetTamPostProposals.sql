-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/18/2010
-- Description:	Retrieves all TamPostProposals for a given TamPost.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostProposals]
	@tam_post_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		tpp.*
	FROM
		tam_post_proposals tpp (NOLOCK)
	WHERE
		tpp.tam_post_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_ids)
		)
END
