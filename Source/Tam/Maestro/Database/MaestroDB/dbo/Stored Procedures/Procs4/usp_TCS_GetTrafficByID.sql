
CREATE Procedure [dbo].[usp_TCS_GetTrafficByID]

      (

            @traffic_id Int

      )

AS

SELECT DISTINCT 
	case when traffic.original_traffic_id is null then traffic.id else traffic.original_traffic_id end, 
	max(traffic.revision), 
	proposals.advertiser_company_id 'advertiser_id', 
	products.name 'product', 
	proposals.agency_company_id 'agency_id', 
	traffic.name 'title',
	(employees.firstname + ' ' + employees.lastname) 'salesperson', 
	traffic.date_created, 
	traffic_employees.effective_date, 
	statuses.name, 
	spot_lengths.length, 
	rate_card_types.name 
	
FROM traffic (NOLOCK) 
	JOIN statuses (NOLOCK) ON traffic.status_id = statuses.id
	JOIN traffic_proposals (NOLOCK) ON traffic_proposals.traffic_id = traffic.id
	JOIN proposals (NOLOCK) ON proposals.id = traffic_proposals.proposal_id
	LEFT JOIN traffic_employees (NOLOCK) ON traffic_employees.traffic_id = traffic.id	
	LEFT JOIN employees (NOLOCK) ON employees.id=traffic_employees.employee_id 	
	LEFT JOIN products (NOLOCK) ON products.id=proposals.product_id 
	LEFT JOIN spot_lengths (NOLOCK) ON spot_lengths.id = proposals.default_spot_length_id
	LEFT JOIN rate_card_types (NOLOCK) ON rate_card_types.id = proposals.rate_card_type_id
	LEFT JOIN traffic_audiences (NOLOCK) ON traffic_audiences.traffic_id = traffic.id 
WHERE 
	statuses.status_set = 'traffic' 
	AND traffic_proposals.primary_proposal = 1 
	AND 
	(cast(traffic.id as varchar) like (cast(@traffic_id as varchar) + '%') 
	 OR
 	 cast(proposals.id as varchar) like (cast(@traffic_id as varchar) + '%'))
	AND (traffic_employees.employee_id <> 183 or traffic_employees.employee_id is null) -- MAESTRO user
	AND traffic.status_id not in (24, 25)
GROUP BY 
	case when traffic.original_traffic_id is null then traffic.id else traffic.original_traffic_id end, 
	proposals.advertiser_company_id,
	products.name,
	proposals.agency_company_id, 
	traffic.name, 
	(employees.firstname + ' ' + employees.lastname),
	traffic.date_created, 
	traffic_employees.effective_date,
	statuses.name,
	spot_lengths.length, 
	rate_card_types.name 
ORDER BY 
	case when traffic.original_traffic_id is null then traffic.id else traffic.original_traffic_id end 
	




