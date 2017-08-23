
CREATE PROCEDURE [dbo].[usp_tam_post_gap_projections_select_all]
AS
SELECT
	*
FROM
	tam_post_gap_projections WITH(NOLOCK)

