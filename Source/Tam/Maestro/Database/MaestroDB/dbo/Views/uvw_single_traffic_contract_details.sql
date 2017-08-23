	
	CREATE VIEW dbo.uvw_single_traffic_contract_details
	AS
	SELECT
		tp.traffic_id,
		pd.network_id,
		pd.proposal_rate,
		pdw.units
	FROM
		dbo.traffic_proposals tp (NOLOCK)
		JOIN dbo.traffic t (NOLOCK) ON t.id=tp.traffic_id
		JOIN dbo.proposal_details pd (NOLOCK) ON pd.proposal_id=tp.proposal_id
		JOIN dbo.proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
		JOIN dbo.media_weeks pmw (NOLOCK) ON pmw.id=pdw.media_week_id
			AND (pmw.start_date <= t.end_date AND pmw.end_date >= t.start_date)
