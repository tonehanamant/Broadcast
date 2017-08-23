-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetOrderedSystems]
	@topography_id INT,
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
    DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	SELECT @start_date=mm.start_date, @end_date=mm.end_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id

    SELECT
		s.system_id,
		s.start_date,
		s.code,
		s.name,
		s.location,
		s.spot_yield_weight,
		s.traffic_order_format,
		s.flag,
		s.active,
		CASE WHEN s.end_date IS NULL THEN @end_date ELSE s.end_date END 'end_date'
	FROM
		uvw_system_universe s
	WHERE
		s.system_id IN (
			SELECT DISTINCT
				so.system_id
			FROM
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
			WHERE
				so.topography_id=@topography_id
		)
		AND s.start_date<=@start_date AND (s.end_date>=@start_date OR s.end_date IS NULL)
	ORDER BY
		s.code
END
