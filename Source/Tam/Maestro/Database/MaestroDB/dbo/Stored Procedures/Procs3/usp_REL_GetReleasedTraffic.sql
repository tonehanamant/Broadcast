-- =============================================
-- Author:		Joe Jacobs
-- Last Edited:	Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE Procedure [dbo].[usp_REL_GetReleasedTraffic]
(
	@release_id int
)
AS
BEGIN
	SELECT
		t.*
	FROM
		traffic t (NOLOCK)
	WHERE
		t.release_id = @release_id
	ORDER BY
		t.sort_order
END