
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayMediaMonths_ByFlight]
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		year,
		month,
		media_month,
		start_date,
		end_date,
		(SELECT COUNT(*) FROM media_weeks (NOLOCK) WHERE media_weeks.media_month_id=media_months.id) 'weeks_in_month'
	FROM 
		media_months (NOLOCK)
	WHERE 
		media_months.start_date <= @end_date AND media_months.end_date >= @start_date
	ORDER BY
		year ASC,
		month ASC

	SELECT
		id,
		media_month_id
	FROM
		media_weeks (NOLOCK) 
	WHERE
		media_weeks.start_date <= @end_date AND media_weeks.end_date >= @start_date
END
