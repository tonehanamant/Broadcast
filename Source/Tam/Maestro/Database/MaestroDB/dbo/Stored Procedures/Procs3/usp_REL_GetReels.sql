

CREATE Procedure [dbo].[usp_REL_GetReels]
  
AS

SELECT
	reels.*
FROM
	reels (NOLOCK)
ORDER BY
	reels.name

