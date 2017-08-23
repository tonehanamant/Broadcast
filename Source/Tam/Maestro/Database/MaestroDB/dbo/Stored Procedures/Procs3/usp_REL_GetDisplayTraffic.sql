

/******  END MME-1302- cancellation ******/


CREATE Procedure [dbo].[usp_REL_GetDisplayTraffic]
      (
            @traffic_id int
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
		proposals.product_id [product_id], 
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
							 ORDER  BY COUNT(*) DESC)
	from traffic WITH (NOLOCK) 
	left join statuses WITH (NOLOCK) on statuses.id = traffic.status_id
	left join traffic_proposals WITH (NOLOCK) on traffic.id = traffic_proposals.traffic_id and traffic_proposals.primary_proposal = 1
	left join proposals WITH (NOLOCK) on proposals.id = traffic_proposals.proposal_id
	left join products WITH (NOLOCK) on proposals.product_id = products.id
	where traffic.id = @traffic_id
	order by traffic.sort_order
END

