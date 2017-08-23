CREATE PROCEDURE usp_PCS_MO_GetNetworkDetailLineNonGuaranteeAudienceDelivery        
(        
@proposal_detail_id int        
,@tam_post_id int      
)        
AS         
SELECT         
	(round(((((dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, pa.audience_id)) / dbo.GetTotalGuaranteeImpressionsByProposalNetworkAudience(p.id, pa.audience_id, pd.network_id)) 
		* dbo.GetPostDeliveryByProposalNetworkAudience(pd.network_id, p.id, pa.audience_id, @tam_post_id)) 
		/ (CASE WHEN pd.num_spots = 0 THEN 1 ELSE pd.num_spots END)) * 10.0, 0))  AS demo_delivery
	,pa.ordinal        
FROM        
	proposals p (NOLOCK)        
	INNER JOIN proposal_details pd (NOLOCK) ON pd.proposal_id = p.id        
	INNER JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id = p.id 
		AND pa.ordinal <> p.guarantee_type        
WHERE        
	pd.id = @proposal_detail_id 
	AND pa.audience_id <> 31         
ORDER BY        
	pa.ordinal   
