
CREATE PROCEDURE [dbo].[usp_MCS_GetReels]
AS
BEGIN
    SELECT	
		*
	FROM 
		reels (NOLOCK)
	ORDER BY 
		date_created
END
