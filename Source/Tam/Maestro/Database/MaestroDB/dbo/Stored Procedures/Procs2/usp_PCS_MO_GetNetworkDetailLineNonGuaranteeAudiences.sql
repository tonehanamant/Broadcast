CREATE PROCEDURE usp_PCS_MO_GetNetworkDetailLineNonGuaranteeAudiences  
(  
 @proposal_detail_id int  
)  
AS   
SELECT   
 round(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, pa.audience_id) * 10.0, 0),  
 pa.ordinal  
FROM  
 proposals p (NOLOCK)  
 join proposal_details pd (NOLOCK) on pd.proposal_id = p.id  
 join proposal_audiences pa (NOLOCK) on pa.proposal_id = p.id and pa.ordinal <> p.guarantee_type  
WHERE  
 pd.id = @proposal_detail_id AND pa.audience_id <> 31   
ORDER BY  
 pa.ordinal  
 
