-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_SetQuarterlyInventory 104,408594,2013,4
CREATE PROCEDURE [dbo].[usp_FOG_SetQuarterlyInventory]
	@topography_id INT,
	@total_quarterly_dollars MONEY,
	@year INT,
	@quarter INT
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @base_media_month_id INT
    DECLARE @media_month_id INT
	DECLARE MonthCursor CURSOR FAST_FORWARD FOR
		SELECT 
			mm.id
		FROM
			media_months mm (NOLOCK)
		WHERE
			mm.[year]=@year
			AND @quarter=CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END
		ORDER BY
			mm.start_date
			
			
	SELECT
		@base_media_month_id = mm.id
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.media_month = (SELECT value FROM properties WHERE name='latest_base_month')


	OPEN MonthCursor

	FETCH NEXT FROM MonthCursor INTO @media_month_id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			EXEC dbo.usp_FOG_SetMonthlyInventoryRates @topography_id,@media_month_id,@total_quarterly_dollars,@base_media_month_id
			FETCH NEXT FROM MonthCursor INTO @media_month_id
		END

	CLOSE MonthCursor
	DEALLOCATE MonthCursor
END

