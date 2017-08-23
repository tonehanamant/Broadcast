CREATE PROCEDURE dbo.usp_proposal_detail_worksheets_GetProposalsUnits 
	@networkdIds XML,
	@mediaWeekIds XML
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @latestFrozenMediaMonth int
	set @latestFrozenMediaMonth = (select max(media_month_id) from frozen_proposal_mvpd_topography_map)

	SELECT 
		pd.network_id,
		pdws.media_week_id,
		pd.daypart_id,   
		frz.mvpd_business_id,
		Sum(pdws.units) as 'NotPlannedProposalUnits'
	FROM proposal_detail_worksheets pdws
		INNER JOIN proposal_details pd 
			on pdws.proposal_detail_id = pd.id
		INNER JOIN proposals p 
			on pd.proposal_id = p.id
			AND p.proposal_status_id in (4,8,9)
		LEFT JOIN traffic_proposals tp 	
			on tp.proposal_id = p.id 
		LEFT JOIN traffic_flight_weeks tfw 	
			on tfw.traffic_id = tp.traffic_id and pdws.media_week_id = tfw.media_week_id	
		-- Looking to check if the proposal is on the ignore list
		LEFT JOIN proposal_settings ps
			on ps.proposal_id = p.id
		INNER JOIN proposal_topographies pt 
			on pt.proposal_id = p.id
		INNER JOIN frozen_proposal_mvpd_topography_map frz 
			on frz.proposal_topography_id = pt.topography_id
			and frz.media_month_id = @latestFrozenMediaMonth
	WHERE 
		tfw.media_week_id IS NULL 
		-- The Proposal is not on the ignore list.
		AND ( ps.hide_proposal_in_traffic_planning_app IS NULL OR ps.hide_proposal_in_traffic_planning_app = 0 )
		AND pdws.media_week_id IN (SELECT x.y.value('.','int') AS id FROM @mediaWeekIds.nodes('/mediaWeeks/id') AS x (y))
		AND pd.network_id IN (SELECT x.y.value('.','int') AS id FROM @networkdIds.nodes('/networks/id') AS x (y))
		-- There are no Married Proposals that cover the same Flight Week
		AND p.id NOT in (
			select 
				p.id
				from proposals p
				join proposal_proposals pp
					on pp.child_proposal_id = p.id
				join uvw_proposal_flights c_pf
					on c_pf.proposal_id = pp.child_proposal_id
				join uvw_proposal_flights p_pf
					on p_pf.proposal_id = pp.parent_proposal_id
				where p.id = pd.proposal_id
				-- Make sure parent and child proposals link on media week
				and c_pf.media_week_id = p_pf.media_week_id
				-- The media week we are looking for a parent
				and c_pf.media_week_id = pdws.media_week_id
		)
	GROUP BY
		pd.network_id,
		pd.daypart_id,  
		pdws.media_week_id,
		frz.mvpd_business_id
	 ORDER BY pdws.media_week_id,pd.network_id
END