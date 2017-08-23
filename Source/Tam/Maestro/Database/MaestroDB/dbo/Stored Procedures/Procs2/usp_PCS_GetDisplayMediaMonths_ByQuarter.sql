-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/20/2010
-- Description: Gets media months for display based on the year and quarter.
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayMediaMonths_ByQuarter]
	@quarter SMALLINT,
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		[year],
		[month],
		media_month,
		start_date,
		end_date,
		(SELECT COUNT(*) FROM media_weeks (NOLOCK) WHERE media_weeks.media_month_id=mm.id) 'weeks_in_month'
	FROM 
		media_months mm (NOLOCK)
	WHERE 
		@quarter = CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END
		AND mm.[year] = @year
	ORDER BY
		[year] ASC,
		[month] ASC

	SELECT
		mw.id,
		mw.media_month_id
	FROM
		media_weeks mw (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		@quarter = CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END
		AND mm.[year] = @year
END
