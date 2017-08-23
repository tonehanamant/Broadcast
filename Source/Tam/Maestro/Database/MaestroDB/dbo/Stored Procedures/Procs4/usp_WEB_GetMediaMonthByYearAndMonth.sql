-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_WEB_GetMediaMonthByYearAndMonth
	@year INT,
	@month INT
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
		mm.year = @year
		AND mm.month = @month
END
