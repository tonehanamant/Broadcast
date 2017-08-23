-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MCS_GetReelsWithScreeners
AS
BEGIN
	SELECT
		r.*
	FROM
		dbo.reels r (NOLOCK)
	WHERE
		r.has_screener=1
END
