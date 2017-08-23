
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalCandidateList]
	@media_month_id int,
	@spot_length_id int 	
AS
BEGIN
	select 
		proposals.advertiser_company_id, 
		proposals.product_id,    
		proposals.id, 
		proposals.original_proposal_id, 
		proposals.version_number, 
		proposals.name,   
		proposals.flight_text,
		proposals.agency_company_id,
		proposals.is_equivalized,
		proposal_audiences.audience_id,
		proposals.rate_card_type_id,
		proposals.base_ratings_media_month_id,
		proposals.base_universe_media_month_id,
		proposal_statuses.name
	from 
		proposals (NOLOCK)
		join proposal_statuses WITH (NOLOCK) on proposal_statuses.id = proposals.proposal_status_id 
		join spot_lengths (NOLOCK) on spot_lengths.id = proposals.default_spot_length_id
		join media_months (NOLOCK) on (proposals.start_date between media_months.start_date and media_months.end_date)
			or (proposals.end_date between media_months.start_date and media_months.end_date)
			or (proposals.start_date < media_months.start_date and proposals.end_date > media_months.end_date)
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id 
		and cast(proposals.guarantee_type as bit) = proposal_audiences.ordinal 
	where
		spot_lengths.id = @spot_length_id
		and media_months.id = @media_month_id 
		-- filter out posting plans
		and proposal_status_id <> 7
		-- filter OUT all media plans that have already been married
		and proposals.id not in (
			select proposal_id from proposal_marry_candidates (NOLOCK) where spot_length_id = @spot_length_id and media_month_id = @media_month_id
		)
		-- filter OUT all media plans with mutlti daypart for the same network
		AND proposals.id not in (
			SELECT
				tmp.proposal_id
			FROM (
				SELECT
					pd.proposal_id,
					pd.daypart_id,
					pd.network_id
				FROM
					proposal_details pd (NOLOCK)
				GROUP BY
					pd.proposal_id,
					pd.daypart_id,
					pd.network_id
			) tmp
			GROUP BY
				tmp.proposal_id,
				tmp.network_id
			HAVING
				COUNT(1)>1
		)
	order by
		proposals.advertiser_company_id
END
