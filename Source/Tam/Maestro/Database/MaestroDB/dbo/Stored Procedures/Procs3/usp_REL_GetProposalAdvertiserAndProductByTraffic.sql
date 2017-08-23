
CREATE PROCEDURE [dbo].[usp_REL_GetProposalAdvertiserAndProductByTraffic]
	@traffic_id int
AS
	SELECT 
		proposals.id, 
		proposals.original_proposal_id, 
		products.name,
		proposals.advertiser_company_id,
		proposals.product_id,
		proposals.advertiser_company_id,
		proposals.agency_company_id,
		proposals.agency_company_id,
		products.description,
		traffic.name,
		traffic.description, 
		traffic.display_name,
		release_product_descriptions.product_description
	FROM proposals 
		join traffic_proposals (NOLOCK) on traffic_proposals.proposal_id = proposals.id and traffic_proposals.primary_proposal = 1
		join traffic (NOLOCK) on traffic_proposals.traffic_id = traffic.id
		left join products (NOLOCK) on products.id = proposals.product_id 
		left join release_product_descriptions (NOLOCK) on release_product_descriptions.id = traffic.product_description_id
	WHERE 
		traffic_proposals.traffic_id = @traffic_id
