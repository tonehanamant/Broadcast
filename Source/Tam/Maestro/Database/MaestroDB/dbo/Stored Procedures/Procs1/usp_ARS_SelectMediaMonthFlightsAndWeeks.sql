-- usp_ARS_SelectMediaMonthFlightsAndWeeks '7/16/2013','9/29/2013'
CREATE PROCEDURE [dbo].[usp_ARS_SelectMediaMonthFlightsAndWeeks] 
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		mm.id,
		(
			SELECT
				COUNT(1) 
			FROM 
				media_weeks mw (NOLOCK) 
			WHERE 
				mm.start_date <= mw.end_date AND mm.end_date >= mw.start_date 
				AND mw.start_date <= @end_date AND mw.end_date >= @start_date
		) 'active_weeks_in_media_month',
		mm.start_date,
		mm.end_date
	FROM 
		media_months mm (NOLOCK)
	WHERE
		mm.start_date <= @end_date AND mm.end_date >= @start_date
END
