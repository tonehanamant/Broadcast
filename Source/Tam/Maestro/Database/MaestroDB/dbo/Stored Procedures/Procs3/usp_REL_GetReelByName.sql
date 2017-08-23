CREATE Procedure [dbo].[usp_REL_GetReelByName]
(
	@name as varchar(63)

)
AS

SELECT
	reels.*
FROM
	reels (NOLOCK)
WHERE
	reels.name = @name
ORDER BY
	reels.name
