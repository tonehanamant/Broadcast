

CREATE Procedure [dbo].[usp_REL_GetProductLineupForReleaseAndMvpd]
(
	@release_id int,
	@mvpd_business_id int
)
AS
select 
      distinct 
      traffic.id, 
      traffic.description,
      case when products.display_name is null or products.display_name = '' 
	then 
		products.name 
	else 
		products.display_name
      end,
      spot_lengths.length, 
      min(cluster_spot_targets.start_date), 
      max(cluster_spot_targets.end_date),
      case when release_product_descriptions.product_description is null or release_product_descriptions.product_description = ''
	then 
		products.description 
	else 
		release_product_descriptions.product_description 
      end,
      proposals.advertiser_company_id,
      case when traffic.display_name is null or traffic.display_name = '' 
	then 
		traffic.name 
	else 
		traffic.display_name end,
      traffic.sort_order
from 
      traffic (NOLOCK)
      join traffic_proposals (NOLOCK) 
            on traffic.id = traffic_proposals.traffic_id
      join proposals (NOLOCK) 
            on proposals.id = traffic_proposals.proposal_id 
            and traffic_proposals.primary_proposal = 1
      join traffic_details (NOLOCK)
            on traffic_details.traffic_id = traffic.id
      join spot_lengths (NOLOCK) 
            on spot_lengths.id = (SELECT TOP 1 WITH ties traffic_details.spot_length_id
                                                FROM traffic_details
                                                WHERE traffic_details.spot_length_id IS Not NULL
                                                AND traffic_details.traffic_id = traffic.id
                                                GROUP BY traffic_details.spot_length_id
                                                ORDER  BY COUNT(*) DESC)
      join cluster_spot_targets (NOLOCK)
            on cluster_spot_targets.traffic_id = traffic.id
      join cluster_allocation_groups (NOLOCK)
            on cluster_allocation_groups.id = cluster_spot_targets.cluster_allocation_group_id
      left join products (NOLOCK) 
            on products.id = proposals.product_id
      left join release_product_descriptions (NOLOCK) 
            on release_product_descriptions.id = traffic.product_description_id
where
      traffic.release_id = @release_id
      and cluster_spot_targets.spots > 0
      and cluster_allocation_groups.mvpd_business_id = @mvpd_business_id
group by
      traffic.id, 
      traffic.description,
      case when products.display_name is null or products.display_name = '' then products.name else products.display_name end,
      spot_lengths.length, 
      case when release_product_descriptions.product_description is null or release_product_descriptions.product_description = '' then products.description else release_product_descriptions.product_description end,
      proposals.advertiser_company_id,
      case when traffic.display_name is null or traffic.display_name = '' then traffic.name else traffic.display_name end,
      traffic.sort_order
order by
      traffic.sort_order, 
      proposals.advertiser_company_id, 
      min(cluster_spot_targets.start_date), 
      max(cluster_spot_targets.end_date)