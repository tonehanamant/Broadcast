-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/28/2012
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPostsByIds '100002,100003'
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPostsByIds]
	@tam_post_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.id IN (
			SELECT id FROM dbo.SplitIntegers(@tam_post_ids)
		)
END
