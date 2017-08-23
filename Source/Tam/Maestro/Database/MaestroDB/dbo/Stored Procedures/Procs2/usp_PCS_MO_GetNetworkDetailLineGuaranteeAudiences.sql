CREATE PROCEDURE usp_PCS_MO_GetNetworkDetailLineGuaranteeAudiences  
(  
 @proposal_id int  
)  
AS   
SELECT   
 round(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, pa.audience_id) * 10.0, 0),  
 round(dbo.GetProposalDetailCPMUnEquivalized(pd.id, pa.audience_id), 2) [Blended CPM],  
 'N',  
 round(dbo.GetProposalDetailDeliveryUnEquivalized(pd.id, 31) * 10.0, 0) [HH impressions],  
 n.name [Network Name],  
 n.code [Network Code],  
 pa.ordinal,  
 CASE a.sub_category_code   
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
 END [audience_string],  
 pd.id  
FROM  
 proposals p (NOLOCK)  
 join proposal_details pd (NOLOCK) on pd.proposal_id = p.id  
 join proposal_audiences pa (NOLOCK) on pa.proposal_id = p.id and pa.ordinal = p.guarantee_type  
 join proposal_detail_audiences pda (NOLOCK) on pd.id = pda.proposal_detail_id and pa.audience_id = pda.audience_id  
 join audiences a (NOLOCK) on a.id = pa.audience_id  
 join uvw_network_universe n (NOLOCK) on n.network_id = pd.network_id and (n.start_date<=pd.start_date AND (n.end_date>=pd.start_date OR n.end_date IS NULL))  
   
WHERE  
 p.id = @proposal_id and pd.include = 1  
ORDER BY  
 n.code, pd.id, pa.ordinal  
 
