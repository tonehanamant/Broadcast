-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/8/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostGapProjections]
	@tam_post_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		gp.*
	FROM
		tam_post_gap_projections gp (NOLOCK)
	WHERE
		gp.tam_post_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_ids)
		)
END
