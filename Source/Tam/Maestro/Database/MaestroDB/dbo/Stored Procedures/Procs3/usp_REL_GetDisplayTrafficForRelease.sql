
CREATE PROCEDURE [dbo].[usp_REL_GetDisplayTrafficForRelease]
(
    @release_id int
)
	AS
	BEGIN
		select traffic.id, 
		                case 
		                                when traffic.original_traffic_id IS NULL 
		                                then traffic.id 
		                                else traffic.original_traffic_id 
		                end [alt_id],
		                 traffic.revision, 
		                case 
		                                when traffic.display_name is null OR traffic.display_name = '' 
		                                then traffic.name 
		                                else traffic.display_name 
		                end [display_name],
		                proposals.advertiser_company_id [company_id], 
		                products.name [product_name], 
		                traffic.start_date, 
		                traffic.end_date, 
		                statuses.name [status],
		                (SELECT  TOP 1 WITH ties spot_lengths.length
		                    FROM  traffic_details td WITH (NOLOCK)
		                    JOIN spot_lengths WITH (NOLOCK) on spot_lengths.id = td.spot_length_id
		                    WHERE traffic.id = td.traffic_id
		                    GROUP  BY spot_lengths.length
		                    ORDER  BY COUNT(*) DESC),
		                (SELECT  TOP 1 WITH ties td.spot_length_id
		                     FROM  traffic_details td WITH (NOLOCK)
		                     WHERE traffic.id = td.traffic_id
		                     GROUP  BY td.spot_length_id
		                     ORDER  BY COUNT(*) DESC),
		                max(bl.insertion_time),
		                traffic.plan_type
		from traffic WITH (NOLOCK)
		left join traffic_breakdown_statistics bl with (NOLOCK) on bl.traffic_id = traffic.id and bl.system_id is null
		left join statuses WITH (NOLOCK) on statuses.id = traffic.status_id
		left join traffic_proposals WITH (NOLOCK) on traffic.id = traffic_proposals.traffic_id and traffic_proposals.primary_proposal = 1
		left join proposals WITH (NOLOCK) on proposals.id = traffic_proposals.proposal_id
		left join products WITH (NOLOCK) on proposals.product_id = products.id
		where traffic.release_id = @release_id 
		group by
		                traffic.id,              
		                traffic.original_traffic_id, 
		                traffic.revision, 
		                traffic.display_name,
		                traffic.name, 
		                proposals.advertiser_company_id, 
		                products.name, 
		                traffic.start_date, 
		                traffic.end_date, 
		                statuses.name,
		                traffic.plan_type,
		                traffic.sort_order
		order by traffic.sort_order
	END
