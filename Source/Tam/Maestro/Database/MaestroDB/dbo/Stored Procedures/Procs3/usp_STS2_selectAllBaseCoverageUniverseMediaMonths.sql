-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectAllBaseCoverageUniverseMediaMonths]
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
		media_months
	WHERE
		id IN (SELECT DISTINCT base_media_month_id FROM coverage_universes WHERE date_approved IS NOT NULL)
	ORDER BY
		id DESC
END
