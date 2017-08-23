-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/22/2011
-- Description:	Returns list of released traffic_id's.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetReleasedTraffic]
AS
BEGIN
	SELECT
		t.id,
		MIN(tdw.start_date),
		MAX(tdw.end_date)
	FROM
		traffic t WITH(NOLOCK)
		JOIN traffic_details td WITH(NOLOCK) ON td.traffic_id=t.id
		JOIN traffic_detail_weeks tdw WITH(NOLOCK) ON tdw.traffic_detail_id=td.id
	WHERE
		t.release_id IS NOT NULL
	GROUP BY
		t.id
	ORDER BY
		t.id DESC
END
