-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayMediaWeeksForFlight]
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		mw.id,
		mw.week_number,
		mm.id,
		mm.year,
		mm.month,
		mw.start_date,
		mw.end_date,
		mm.start_date,
		mm.end_date
	FROM
		media_weeks mw (NOLOCK) 
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		(mw.start_date <= @end_date AND mw.end_date >= @start_date)
	ORDER BY
		mw.start_date ASC
END
