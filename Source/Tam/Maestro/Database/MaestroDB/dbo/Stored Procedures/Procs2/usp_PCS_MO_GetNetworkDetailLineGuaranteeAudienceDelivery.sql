CREATE PROCEDURE usp_PCS_MO_GetNetworkDetailLineGuaranteeAudienceDelivery          
(          
	@proposal_id INT          
	,@tam_post_id INT        
)          
AS           
SELECT           
	(ROUND(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, pa.audience_id), 0) / dbo.GetTotalGuaranteeImpressionsByProposalNetworkAudience(p.id, pa.audience_id, n.network_id)) AS demo_impression_ratio     
	,(ROUND(((((dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, pa.audience_id)) / dbo.GetTotalGuaranteeImpressionsByProposalNetworkAudience(p.id, pa.audience_id, n.network_id))   
		* dbo.GetPostDeliveryByProposalNetworkAudience(pd.network_id, p.id, pa.audience_id, @tam_post_id))   
		/ (CASE WHEN pd.num_spots = 0 THEN 1 ELSE pd.num_spots END)) * 10.0, 0))  AS demo_delivery       
	,(ROUND(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, 31), 0) / dbo.GetTotalGuaranteeImpressionsByProposalNetworkAudience(p.id, 31, n.network_id)) AS hh_impression_ratio
	,(ROUND(((((dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, 31)) / dbo.GetTotalGuaranteeImpressionsByProposalNetworkAudience(p.id, 31, n.network_id))   
		* dbo.GetPostDeliveryByProposalNetworkAudience(pd.network_id, p.id, 31, @tam_post_id))
		/ (CASE WHEN pd.num_spots = 0 THEN 1 ELSE pd.num_spots END)) * 10.0, 0)) AS hh_delivery
	,n.code [Network Code]
	,pa.ordinal        
	,CASE a.sub_category_code           
		WHEN 'A' THEN 'P'           
		WHEN 'K' THEN 'P'          
		WHEN 'H' THEN 'P'          
		WHEN 'O' THEN 'P'          
		WHEN 'T' THEN 'P'          
		WHEN 'W' THEN 'F'          
		WHEN ' ' THEN 'M'          
		ELSE a.sub_category_code           
	END          
	+          
	RIGHT('0'+CONVERT(VARCHAR(2), a.range_start), 2)          
	+          
	CASE range_end          
		WHEN 99 THEN '+'          
		ELSE          
			RIGHT('0'+CONVERT(VARCHAR(2), a.range_end), 2)          
	END [audience_string]         
	,pd.id          
FROM          
proposals p (NOLOCK)          
INNER JOIN proposal_details pd (NOLOCK) ON pd.proposal_id = p.id          
INNER JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id = p.id 
	AND pa.ordinal = p.guarantee_type          
INNER JOIN proposal_detail_audiences pda (NOLOCK) ON pd.id = pda.proposal_detail_id 
	AND pa.audience_id = pda.audience_id          
INNER JOIN audiences a (NOLOCK) ON a.id = pa.audience_id          
INNER JOIN uvw_network_universe n (NOLOCK) ON n.network_id = pd.network_id 
	AND (n.start_date<=pd.start_date AND (n.end_date>=pd.start_date OR n.end_date IS NULL))  
WHERE          
	p.id = @proposal_id 
	AND pd.include = 1          
ORDER BY          
	n.code
	,pd.id
	,pa.ordinal          
