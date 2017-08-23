-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/14/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetMediaMonths
AS
BEGIN
	SELECT DISTINCT
		mm.*
	FROM
		media_months mm (NOLOCK)
		JOIN cmw_traffic_flights ctf (NOLOCK) ON (mm.start_date <= ctf.end_date AND mm.end_date >= ctf.start_date) AND ctf.selected=1
	ORDER BY
		mm.start_date DESC
END
