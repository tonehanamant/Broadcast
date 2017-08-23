-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/20/2010
-- Description:	<Description,,>
-- =============================================
-- usp_PCS_GetDisplayPost 100001
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayPost]
	@tam_post_id INT
AS
BEGIN
	SELECT
		dp.*
	FROM
		uvw_display_posts dp
	WHERE
		dp.id = @tam_post_id
END
