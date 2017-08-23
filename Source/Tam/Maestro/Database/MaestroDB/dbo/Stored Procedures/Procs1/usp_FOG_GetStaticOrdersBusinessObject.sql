-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/23/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_GetStaticOrdersBusinessObject 385,104
CREATE PROCEDURE [dbo].[usp_FOG_GetStaticOrdersBusinessObject]
	@media_month_id INT,
	@topography_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
	-- media_weeks
	SELECT DISTINCT
		mw.*
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
	ORDER BY
		mw.start_date
	
	-- systems
	SELECT DISTINCT
		si.system_id,
		s.code
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN uvw_system_universe s ON s.system_id=si.system_id AND s.start_date<=mw.start_date AND (s.end_date>=mw.start_date OR s.end_date IS NULL)
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
	ORDER BY
		s.code
		
	-- zones
	SELECT DISTINCT
		si.zone_id,
		z.code,
		z.name
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN uvw_zone_universe z ON z.zone_id=si.zone_id AND z.start_date<=mw.start_date AND (z.end_date>=mw.start_date OR z.end_date IS NULL)
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
	ORDER BY
		z.code
		
	-- zone networks
	SELECT DISTINCT
		si.zone_id,
		zn.network_id,
		CASE WHEN zn.subscribers IS NULL THEN 0 ELSE zn.subscribers END 'subscribers'
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN media_months mm (NOLOCK) ON mm.id=@media_month_id
		LEFT JOIN uvw_zonenetwork_universe zn ON zn.zone_id=si.zone_id AND zn.network_id=si.network_id AND zn.start_date<=mm.start_date AND (zn.end_date>=mm.start_date OR zn.end_date IS NULL)
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
	ORDER BY
		si.zone_id,
		zn.network_id
		
	-- networks
	SELECT DISTINCT
		si.network_id,
		n.code,
		n.name
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN uvw_network_universe n ON n.network_id=si.network_id AND n.start_date<=mw.start_date AND (n.end_date>=mw.start_date OR n.end_date IS NULL)
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
	ORDER BY
		n.code
		
	-- display proposals
	SELECT DISTINCT
		dp.*
	FROM
		static_orders so (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN uvw_display_proposals dp ON dp.id=so.proposal_id
	WHERE
		so.topography_id=@topography_id

	-- inventory dayparts	
	SELECT DISTINCT
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN vw_ccc_daypart d ON d.id=si.daypart_id
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
		
	-- ordered dayparts	
	SELECT DISTINCT
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun
	FROM
		static_inventory_dayparts stid (NOLOCK)
		JOIN vw_ccc_daypart d ON d.id=stid.daypart_id
	WHERE
		stid.topography_id=@topography_id

	-- raw static inventory
	SELECT
		si.topography_id, 
		si.system_id, 
		si.media_week_id, 
		si.zone_id, 
		si.network_id, 
		si.daypart_id, 
		si.available_units,
		CASE WHEN zn.subscribers IS NULL THEN 0 ELSE zn.subscribers END 'subscribers'
	FROM
		static_inventories si (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=si.media_week_id
			AND mw.media_month_id=@media_month_id
		JOIN media_months mm (NOLOCK) ON mm.id=@media_month_id
		LEFT JOIN uvw_zonenetwork_universe zn ON zn.zone_id=si.zone_id AND zn.network_id=si.network_id AND zn.start_date<=mm.start_date AND (zn.end_date>=mm.start_date OR zn.end_date IS NULL)
	WHERE
		si.topography_id=@topography_id
		AND si.available_units>0
		AND si.enable=1
END
