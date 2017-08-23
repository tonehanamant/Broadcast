
-- SELECT * FROM uvw_analysis_delivered_hh_rating WHERE media_month='0709'
CREATE VIEW [dbo].[uvw_analysis_delivered_HH_rating]
AS

SELECT 
	media_months.media_month,
	products.name 'product',
	materials.code 'copy',
	traffic_proposals.proposal_id, 
	networks.code 'network',
	spot_lengths.length,
	SUM((CAST(affidavits.subscribers AS FLOAT) / ad_hh.universe) * ad_hh.audience_usage) 'total_hh_impressions',
	SUM(affidavits.subscribers) 'total_subscribers', 
	SUM(ad_hh.audience_usage / ad_hh.universe) 'total_delivered_hh_rating'
FROM 
	affidavits (NOLOCK)
	LEFT JOIN affidavit_deliveries ad_hh ON ad_hh.affidavit_id=affidavits.id AND ad_hh.audience_id=31
	JOIN invoices (NOLOCK) ON invoices.id=affidavits.invoice_id
	JOIN media_months ON media_months.id=invoices.media_month_id
	JOIN networks (NOLOCK) ON networks.id = affidavits.network_id 
	JOIN materials (NOLOCK) ON materials.id = affidavits.material_id
	JOIN traffic_materials (NOLOCK) ON traffic_materials.material_id = materials.id
	JOIN traffic (NOLOCK) ON traffic.id = traffic_materials.traffic_id
	JOIN traffic_proposals (NOLOCK) ON traffic.id = traffic_proposals.traffic_id 
	JOIN proposals (NOLOCK) ON proposals.id = traffic_proposals.proposal_id
	JOIN spot_lengths (NOLOCK) ON spot_lengths.id = affidavits.spot_length_id 
	JOIN products (NOLOCK) ON proposals.product_id = products.id
WHERE
	affidavits.status_id=1
GROUP BY
	media_months.media_month,
	products.name, 
	materials.code,
	traffic_proposals.proposal_id, 
	networks.code,
	spot_lengths.length