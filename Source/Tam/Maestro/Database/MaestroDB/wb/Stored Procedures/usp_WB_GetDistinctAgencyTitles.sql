CREATE PROCEDURE [wb].[usp_WB_GetDistinctAgencyTitles]
(
	@start_date DATETIME,
	@region_code VARCHAR(127)
)
AS
BEGIN
	SELECT 
		DISTINCT 
		wba.name, 
		wbs.order_title
	FROM 
		wb.wb_schedules (NOLOCK) AS wbs
		JOIN dbo.media_weeks (NOLOCK) AS mw ON wbs.media_week_id = mw.id 
		JOIN wb.wb_agencies (NOLOCK) AS wba ON wba.id = wbs.wb_agency_id 
	WHERE
		mw.start_date = @start_date
			AND wbs.region_code = @region_code
	ORDER BY
		wba.name,
		wbs.order_title
END
