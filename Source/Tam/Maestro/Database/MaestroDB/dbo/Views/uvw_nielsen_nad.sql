CREATE VIEW [dbo].[uvw_nielsen_nad]
AS
	SELECT 
		nad.start_date,
		nad.end_date,
		nad.market_break,
		nad.network_id,
		nad.audience_id,
		nad.ue 'universe',
		nad.mc_us_aa_perc 'rating',
		(nad.mc_us_aa_perc * nad.ue)/100.0 'delivery',
		composite.ue 'composite_universe',
		composite.mc_us_aa_perc 'composite_rating',
		(composite.mc_us_aa_perc * composite.ue)/100.0 'composite_delivery',
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
		dbo.nielsen_nads nad (NOLOCK)
		JOIN dbo.vw_ccc_daypart d ON d.id=nad.daypart_id
		JOIN dbo.nielsen_nads composite (NOLOCK) ON composite.start_date=nad.start_date
			AND composite.end_date=nad.end_date
			AND composite.market_break='Composite'
			AND composite.audience_id=nad.audience_id
			AND composite.network_id=nad.network_id
			AND composite.daypart_id=nad.daypart_id
	WHERE
		nad.market_break IN (
			'Territory = East Central',
			'Territory = Northeast',
			'Territory = Pacific',
			'Territory = Southeast',
			'Territory = Southwest',
			'Territory = West Central'
		)
