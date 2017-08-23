-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/7/2010
-- Description:	Retrieves all TamPostReportOptions for a given TamPost.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostReportOptions]
	@tam_post_ids VARCHAR(MAX)
AS
BEGIN
    SELECT
		tpro.*
	FROM
		tam_post_report_options tpro (NOLOCK)
	WHERE
		tpro.tam_post_id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_ids)
		)
END
