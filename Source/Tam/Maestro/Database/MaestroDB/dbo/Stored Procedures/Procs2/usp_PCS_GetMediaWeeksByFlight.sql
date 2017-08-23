-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetMediaWeeksByFlight
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id,
		media_month_id,
		week_number,
		start_date,
		end_date
	FROM
		media_weeks mw (NOLOCK)
	WHERE
		mw.start_date <= @end_date AND mw.end_date >= @start_date
END