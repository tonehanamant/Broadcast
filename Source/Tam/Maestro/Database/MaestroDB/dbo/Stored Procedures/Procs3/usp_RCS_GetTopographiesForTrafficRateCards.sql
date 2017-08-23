-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_RCS_GetTopographiesForTrafficRateCards
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		t.*
	FROM
		dbo.topographies t (NOLOCK)
	WHERE
		t.id IN (
			SELECT DISTINCT topography_id FROM dbo.traffic_rate_cards trc (NOLOCK)
		)
	ORDER BY
		t.name
END
