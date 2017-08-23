-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MCS_GetReelsByName
	@name VARCHAR(63)
AS
BEGIN
	SELECT
		r.*
	FROM
		dbo.reels r (NOLOCK)
	WHERE
		r.name=@name
END
