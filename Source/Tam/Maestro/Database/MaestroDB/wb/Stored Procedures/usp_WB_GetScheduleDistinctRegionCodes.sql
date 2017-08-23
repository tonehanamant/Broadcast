CREATE PROCEDURE wb.usp_WB_GetScheduleDistinctRegionCodes
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT DISTINCT wbs.region_code
    FROM wb.wb_schedules (NOLOCK) AS wbs
	ORDER BY
      wbs.region_code ASC
END
