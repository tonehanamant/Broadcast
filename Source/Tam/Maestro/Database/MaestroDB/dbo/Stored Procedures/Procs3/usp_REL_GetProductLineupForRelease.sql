
CREATE Procedure [dbo].[usp_REL_GetProductLineupForRelease]
	@release_id INT
AS
BEGIN
	select 
		traffic.id, 
		traffic.description,
		case when products.display_name is null or products.display_name = '' then products.name else products.display_name end,
		spot_lengths.length, 
		traffic.start_date, 
		traffic.end_date,
		case when release_product_descriptions.product_description is null or release_product_descriptions.product_description = '' then products.description else release_product_descriptions.product_description end,
		proposals.advertiser_company_id,
		case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end
	from 
		traffic (NOLOCK)
		join traffic_proposals (NOLOCK) 
			on traffic.id = traffic_proposals.traffic_id
		join proposals (NOLOCK) 
			on proposals.id = traffic_proposals.proposal_id and traffic_proposals.primary_proposal = 1
		join spot_lengths (NOLOCK) 
			on spot_lengths.id = proposals.default_spot_length_id
		left join products (NOLOCK) 
			on products.id = proposals.product_id
		left join release_product_descriptions (NOLOCK) 
			on release_product_descriptions.id = traffic.product_description_id
	where
		traffic.release_id = @release_id
	order by
		traffic.sort_order, 
		proposals.advertiser_company_id, 
		traffic.start_date, 
		traffic.end_date
END