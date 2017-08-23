-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/15/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetYears
AS
BEGIN
	SELECT DISTINCT
		[year]
	FROM (
		SELECT DISTINCT
			YEAR(ctf.start_date) 'year'
		FROM
			cmw_traffic_flights ctf (NOLOCK)

		UNION ALL

		SELECT DISTINCT
			YEAR(ctf.end_date) 'year'
		FROM
			cmw_traffic_flights ctf (NOLOCK)
	) tmp
	ORDER BY
		[year] DESC
END
