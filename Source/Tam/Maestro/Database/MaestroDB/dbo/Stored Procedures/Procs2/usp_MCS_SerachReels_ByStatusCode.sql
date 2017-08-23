-- =============================================
-- Author:        Nicholas Kheynis
-- Create date: 8/6/2014
-- Description:   <Description,,>
-- =============================================
--usp_MCS_SerachReels_ByStatusCode A, 37
CREATE PROCEDURE [dbo].[usp_MCS_SerachReels_ByStatusCode]
	@search_text VARCHAR(25),
	@status_code TINYINT
AS
BEGIN
    SELECT	
		r.*
	FROM 
		reels r (NOLOCK)
	WHERE
		r.NAME LIKE @search_text + '%'
		AND r.status_code = @status_code
		
	ORDER BY 
		r.name
END
