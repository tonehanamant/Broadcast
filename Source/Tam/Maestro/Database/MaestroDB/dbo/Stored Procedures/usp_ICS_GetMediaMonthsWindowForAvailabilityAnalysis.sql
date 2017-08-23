-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/16/2015
-- Description:	Get's a list of media months used for analyzing availability. Returns -3 to +18 media months.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_GetMediaMonthsWindowForAvailabilityAnalysis]
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @min_start_date DATETIME;
	DECLARE @max_end_date DATETIME;

	SELECT
		@min_start_date = MIN(p.start_date),
		@max_end_date = MAX(p.end_date)
	FROM 
		proposals p (NOLOCK);
		
	SELECT
		mm.*
	FROM
		dbo.media_months mm
	WHERE
		mm.start_date>=@min_start_date AND mm.end_date<=@max_end_date
	ORDER BY
		mm.start_date;
END