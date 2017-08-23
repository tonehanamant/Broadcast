-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 8/6/2014
-- Description:   <Description,,>
-- =============================================
--usp_MCS_SerachReels_ByStatusCode A
CREATE PROCEDURE [dbo].[usp_MCS_SearchReels]
	@search_text VARCHAR(25)
AS
BEGIN
    SELECT	
		r.*
	FROM 
		reels r (NOLOCK)
	WHERE
		r.NAME LIKE @search_text + '%'	
	ORDER BY 
		r.name
END
