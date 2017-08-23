
CREATE PROCEDURE [dbo].[usp_PCS_GetRelatedMarriedProposalsForPlan]
	   @proposal_id int,
	   @media_month_id int
AS
BEGIN
	   select 
			  pp.parent_proposal_id,
			  pp.rotation_percentage,
			  pp2.child_proposal_id,
			  p.name,
			  pr.name, 
			  p.agency_company_id,
			  p.advertiser_company_id,
			  a.name,
			  t.id,
			  pp2.rotation_percentage,
			  pp2.ordinal
	   from
			  proposal_proposals pp WITH (NOLOCK)
			  join proposal_proposals pp2 WITH (NOLOCK) on pp2.parent_proposal_id = pp.parent_proposal_id and pp2.child_proposal_id <> pp.child_proposal_id
			  join proposals p WITH (NOLOCK) on p.id = pp2.child_proposal_id
			  join proposal_media_month_marriage_mappings pmc WITH (NOLOCK) on pmc.proposal_id = pp.parent_proposal_id
			  join proposal_audiences pa WITH (NOLOCK) on pa.proposal_id = p.id and p.guarantee_type = pa.ordinal
			  join audiences a (NOLOCK) ON a.id=pa.audience_id
			  join traffic_proposals tp WITH (NOLOCK) on tp.proposal_id = pp.parent_proposal_id
			  join traffic t WITH (NOLOCK) on t.id = tp.traffic_id and ((t.plan_type = 0 and t.status_id = 14) or (t.plan_type = 1))
			  left join products pr WITH (NOLOCK) on pr.id = p.product_id
	   where
			  pp.child_proposal_id = @proposal_id
			  and
			  pmc.media_month_id = @media_month_id
			  and
			  ((pp.ordinal > 0 and pp2.ordinal = 0) OR (pp.ordinal = 0))
	   order by
			  pp.parent_proposal_id,
			  pp2.ordinal
END
