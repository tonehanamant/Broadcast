
CREATE PROCEDURE [dbo].[usp_PCS_Get15SecondProposalsForMediaMonth]
	@media_month_id int,
	@chattem_only bit
AS
IF (@chattem_only = 0)
BEGIN
	select 
		proposals.advertiser_company_id,
		products.name,    
		proposals.id, 
		proposals.original_proposal_id, 
		proposals.version_number, 
		proposals.name,   
		proposals.flight_text,
		proposals.agency_company_id,
		proposals.is_equivalized,
		proposal_audiences.audience_id,
		a.name
	from 
		proposals (NOLOCK)
		join spot_lengths (NOLOCK) on spot_lengths.id = proposals.default_spot_length_id
		join media_months (NOLOCK) on 
		(proposals.start_date between media_months.start_date and media_months.end_date)
		or (proposals.end_date between media_months.start_date and media_months.end_date)
		or (proposals.start_date < media_months.start_date and proposals.end_date > media_months.end_date)
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id 
		and cast(proposals.guarantee_type as bit) = proposal_audiences.ordinal 
		left join products (NOLOCK) on products.id = proposals.product_id
		join audiences a (NOLOCK) ON a.id=proposal_audiences.audience_id
	where
		spot_lengths.length = 15
		and proposals.include_on_marriage_report = 1
		-- and (proposal_status_id = 4)-- or proposal_status_id = 2 or proposal_status_id = 5)
		and media_months.id = @media_month_id 
		and proposals.advertiser_company_id <> 39680
	order by
		proposals.advertiser_company_id
END
ELSE
BEGIN
	select 
		proposals.advertiser_company_id, 
		products.name,    
		proposals.id, 
		proposals.original_proposal_id, 
		proposals.version_number, 
		proposals.name,   
		proposals.flight_text,
		proposals.agency_company_id,
		proposals.is_equivalized,
		proposal_audiences.audience_id,
		a.name
	from 
		proposals (NOLOCK)
		join spot_lengths (NOLOCK) on spot_lengths.id = proposals.default_spot_length_id
		join media_months (NOLOCK) on (proposals.start_date between media_months.start_date and media_months.end_date)
		or  (proposals.end_date between media_months.start_date and media_months.end_date)
		or (proposals.start_date < media_months.start_date and proposals.end_date > media_months.end_date)
		join proposal_audiences (NOLOCK) on proposals.id = proposal_audiences.proposal_id 
		and cast(proposals.guarantee_type as bit) = proposal_audiences.ordinal 
		left join products (NOLOCK) on products.id = proposals.product_id
		join audiences a (NOLOCK) ON a.id=proposal_audiences.audience_id
	where
		spot_lengths.length = 15
		and proposals.include_on_marriage_report = 1
		-- and proposal_status_id = 4
		and media_months.id = @media_month_id --JAN
		and proposals.advertiser_company_id = 39680 
	order by
		proposals.advertiser_company_id
END
