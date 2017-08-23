	
	CREATE VIEW dbo.uvw_married_proposal_rate_details
	AS
	SELECT
			pp.parent_proposal_id			as 'parent_id',
			pd_parent.network_id			as 'network_id',
			slm.spot_length_id			as 'spot_length_id',
	
			pd_child.proposal_id  			as 'child1_id',
			pd_child.proposal_rate 			as 'child1_proposal_rate',
			pd_child.universal_scaling_factor	as 'child1_universal_scaling_factor',
			pda_child.us_universe 			as 'child1_us_universe',
			pda_child.rating			as 'child1_rating',
			rc_child.weighting_factor 		as 'child1_weighting_factor',
	
			pd_other_child.proposal_id		as 'child2_id',
			pd_other_child.proposal_rate 		as 'child2_proposal_rate',
			pd_other_child.universal_scaling_factor	as 'child2_universal_scaling_factor',
			pda_other_child.us_universe		as 'child2_us_universe',
			pda_other_child.rating			as 'child2_rating',
			rc_other_child.weighting_factor		as 'child2_weighting_factor',
	
			td.release_amount			as 'release_amount',
			pp.rotation_percentage			as 'rotation_percentage'
		from proposal_proposals pp
		join proposal_details pd_parent
			on pd_parent.proposal_id = pp.parent_proposal_id
		join proposal_details pd_child
			on pd_child.proposal_id = pp.child_proposal_id
			and pd_child.network_id = pd_parent.network_id
		join proposal_proposals pp_other_child
			on pp_other_child.parent_proposal_id = pp.parent_proposal_id
			and pp_other_child.child_proposal_id <> pp.child_proposal_id
		join proposal_details pd_other_child
			on pd_other_child.proposal_id = pp_other_child.child_proposal_id
			and pd_other_child.network_id = pd_parent.network_id
		join proposals p
			on p.id = pd_child.proposal_id
		join traffic_details_proposal_details_map tdpdm
		       on tdpdm.proposal_detail_id = pd_parent.id
		join traffic_details td
		       on td.id = tdpdm.traffic_detail_id
		join traffic t
		       on t.id = td.traffic_id
			   and t.release_id is not null -- this will be the latest revision?
		join spot_length_maps slm
			on slm.spot_length_id = td.spot_length_id
			and map_set = 'release_clearance_factor'
		join proposal_audiences pa_child
			on pa_child.proposal_id = pp.child_proposal_id
			and pa_child.ordinal = p.guarantee_type
		join proposal_audiences pa_other_child
			on pa_other_child.proposal_id = pp_other_child.child_proposal_id
			and pa_other_child.ordinal = p.guarantee_type
		join proposal_detail_audiences pda_child
			on pda_child.proposal_detail_id = pd_child.id
			and pda_child.audience_id = pa_child.audience_id
		join proposal_detail_audiences pda_other_child
			on pda_other_child.proposal_detail_id = pd_other_child.id
			and pda_other_child.audience_id = pa_other_child.audience_id
		join release_cpmlink rc_child
			on rc_child.proposal_id = pp.child_proposal_id
			and rc_child.traffic_id = t.id
		join release_cpmlink rc_other_child
			on rc_other_child.proposal_id = pp_other_child.child_proposal_id
			and rc_other_child.traffic_id = t.id
		where t.release_id is not null
