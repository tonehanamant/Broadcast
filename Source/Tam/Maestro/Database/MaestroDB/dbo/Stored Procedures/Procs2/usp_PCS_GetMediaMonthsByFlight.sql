-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetMediaMonthsByFlight
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
		end_date
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.start_date <= @end_date AND mm.end_date >= @start_date
END
