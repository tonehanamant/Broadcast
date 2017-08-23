-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetMediaMonthsForTopography]
	@topography_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		mm.* 
	FROM 
		media_months mm (NOLOCK) 
	WHERE 
		mm.id IN (
			SELECT DISTINCT 
				mw.media_month_id
			FROM
				static_inventories si (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			WHERE
				si.topography_id=@topography_id
		)
	ORDER BY
		mm.start_date
END
