-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPosts
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPosts]
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
END
