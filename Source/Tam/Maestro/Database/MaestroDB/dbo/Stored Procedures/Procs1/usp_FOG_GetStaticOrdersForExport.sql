-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/23/2013
-- Description:	<Description,,>
-- =============================================
-- usp_FOG_GetStaticOrdersForExport 385,104
CREATE PROCEDURE [dbo].[usp_FOG_GetStaticOrdersForExport]
	@media_month_id INT,
	@topography_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
    DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	SELECT @start_date=mm.start_date, @end_date=mm.end_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id

	-- display proposals
	SELECT
		dp.*
	FROM
		uvw_display_proposals dp
	WHERE
		dp.id IN (
			SELECT DISTINCT
				so.proposal_id
			FROM
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
					--AND mw.week_number>=2
			WHERE
				so.topography_id=@topography_id
		)
	ORDER BY
		dp.advertiser,
		dp.product
		
	-- systems
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
					--AND mw.week_number>=2
			WHERE
				so.topography_id=@topography_id
		)
		AND s.start_date<=@start_date AND (s.end_date>=@start_date OR s.end_date IS NULL)
	ORDER BY
		s.code
	
	-- zones
	SELECT
		tmp.system_id,
		z.zone_id,
		z.start_date,
		z.code,
		z.name,
		z.type,
		z.[primary],
		z.traffic,
		z.dma,
		z.flag,
		z.active,
		CASE WHEN z.end_date IS NULL THEN @end_date ELSE z.end_date END 'end_date'
	FROM (
		SELECT DISTINCT
			so.system_id,
			so.zone_id
		FROM
			static_orders so (NOLOCK)
			JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
				AND mw.media_month_id=@media_month_id
				--AND mw.week_number>=2
		WHERE
			so.topography_id=@topography_id
	) tmp
	JOIN uvw_zone_universe z ON z.zone_id=tmp.zone_id AND z.start_date<=@start_date AND (z.end_date>=@start_date OR z.end_date IS NULL)
		
	-- media_weeks
	SELECT
		mw.*
	FROM
		media_weeks mw (NOLOCK)
	WHERE
		mw.media_month_id=@media_month_id
		--AND mw.week_number>=2
	ORDER BY
		mw.start_date

	-- dayparts
	SELECT
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
		vw_ccc_daypart d
	WHERE
		d.id IN (
			SELECT DISTINCT 
				so.daypart_id 
			FROM 
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
					--AND mw.week_number>=2
			WHERE
				so.topography_id=@topography_id
		)
	
	-- nielsen network mappings
	SELECT
		nm.network_id,
		nn.nielsen_network_id,
		nn.start_date,
		nn.network_rating_category_id,
		nn.nielsen_id,
		nn.code,
		nn.name,
		nn.active,
		CASE WHEN nn.end_date IS NULL THEN @end_date ELSE nn.end_date END 'end_date'
	FROM
		network_maps nm (NOLOCK)
		JOIN uvw_nielsen_network_universes nn ON CAST(nn.nielsen_id AS VARCHAR)=nm.map_value
			AND nn.start_date<=@start_date AND (nn.end_date>=@start_date OR nn.end_date IS NULL)
	WHERE
		nm.map_set='Nielsen'
		AND nm.network_id IN (
			SELECT DISTINCT 
				so.network_id 
			FROM 
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
					--AND mw.week_number>=2
			WHERE
				so.topography_id=@topography_id
		)
		
	-- proposal detail data
	SELECT
		so.proposal_id,
		so.network_id,
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
		static_orders so (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
			--AND mw.week_number>=2
		JOIN proposal_details pd (NOLOCK) ON pd.proposal_id=so.proposal_id
			AND pd.network_id=so.network_id
		JOIN vw_ccc_daypart d ON d.id=pd.daypart_id
	WHERE
		so.topography_id=@topography_id
	
	
	-- zone dmas
	SELECT DISTINCT
		so.system_id,
		so.zone_id,
		zd.dma_id
	FROM
		static_orders so (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
			--AND mw.week_number>=2
		JOIN uvw_zonedma_universe zd ON zd.zone_id=so.zone_id
			AND (zd.start_date<=@start_date AND (zd.end_date>=@start_date OR zd.end_date IS NULL))
	WHERE
		so.topography_id=@topography_id
	
	-- order data
	SELECT
		so.proposal_id,
		so.system_id,
		so.media_week_id,
		so.zone_id,
		so.network_id,
		so.daypart_id,
		so.ordered_units,
		so.ordered_rate
	FROM
		static_orders so (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
			--AND mw.week_number>=2
	WHERE
		so.topography_id=@topography_id
	ORDER BY
		so.proposal_id,
		so.system_id,
		so.media_week_id,
		so.zone_id
END


