	-- END SQL FOR TFS #'s: 9462, 9463

	CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficDetailLines]
	(
	      @tid as int
	)
	AS
	BEGIN
	select distinct 
	      td.id, 
	      td.network_id, 
	      n.code, 
	      td.daypart_id,
	      --case when tdt.daypart_id is null then td.daypart_id else tdt.daypart_id end, 
	      spot_lengths.length,
	      tda.traffic_rating,
	      tdpdm.proposal_rate, 
	      tdpdm.proposal_spots, 
	      p.id,
	      pda.us_universe * pd.universal_scaling_factor,
	      pda.rating,
	      tdw.start_date,
	      tdw.end_date,
	      (select tda2.us_universe from traffic_detail_audiences (NOLOCK) as tda2, traffic_details (NOLOCK) as td2 where td2.id = tda2.traffic_detail_id and tda2.audience_id = 31 and td2.id = td.id),
	      tda.us_universe [HH Coverage Universe],
	      pd.proposal_rate,
	      td.daypart_id,
	      t.status_id,
	      ts.start_time,
	      ts.end_time
	from 
	      traffic t (NOLOCK) 
	      join traffic_details td (NOLOCK) 
	            on t.id = td.traffic_id
	      join uvw_network_universe (NOLOCK) n 
	            on n.network_id = td.network_id AND (n.start_date<=t.start_date AND (n.end_date>=t.start_date OR n.end_date IS NULL))
	      join spot_lengths (NOLOCK) 
	            on spot_lengths.id = td.spot_length_id
	      join traffic_detail_weeks tdw (NOLOCK) 
	            on tdw.traffic_detail_id = td.id
	      join traffic_detail_topographies tdt (NOLOCK)
	            on tdt.traffic_detail_week_id = tdw.id
	      join traffic_audiences ta (NOLOCK) 
	            on ta.traffic_id = t.id and ta.audience_id = t.audience_id
	      join traffic_detail_audiences tda (NOLOCK) 
	            on tda.traffic_detail_id = td.id and tda.audience_id = t.audience_id
	      join traffic_proposals tp (NOLOCK) 
	            on tp.traffic_id = t.id
	      join proposals p (NOLOCK) 
	            on p.id = tp.proposal_id
	      join proposal_details pd (NOLOCK) 
	            on pd.proposal_id = p.id
	      join proposal_audiences pa (NOLOCK) 
	            on pa.proposal_id = p.id and t.audience_id = pa.audience_id
	      join proposal_detail_audiences pda (NOLOCK) 
	            on pda.proposal_detail_id = pd.id and pa.audience_id = pda.audience_id 
	      join dayparts dp (NOLOCK)  
	                  on td.daypart_id = dp.id 
	      join timespans ts (NOLOCK)  
	                  on ts.id = dp.timespan_id
	      right join traffic_details_proposal_details_map tdpdm (NOLOCK) 
	            on td.id = tdpdm.traffic_detail_id 
	            and pd.id = tdpdm.proposal_detail_id
	where
	      t.id = @tid
	ORDER BY 
	        n.code, 
	      tdw.start_date
	
	
	END
