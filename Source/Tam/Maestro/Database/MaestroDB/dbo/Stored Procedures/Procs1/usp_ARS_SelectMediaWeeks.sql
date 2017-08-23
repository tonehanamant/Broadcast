-- usp_ARS_SelectMediaWeeks '7/16/2013','9/29/2013'
CREATE PROCEDURE [dbo].[usp_ARS_SelectMediaWeeks] 
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		mw.*
	FROM 
		media_weeks mw (NOLOCK) 
	WHERE
		mw.start_date <= @end_date AND mw.end_date >= @start_date
	ORDER BY
		mw.start_date
END
