CREATE FUNCTION [dbo].[GetProposalDetailDeliveryUnEquivalized]  
(  
 @proposal_detail_id INT,  
 @audience_id INT  
)  
RETURNS FLOAT  
AS  
BEGIN  
 DECLARE @return AS FLOAT  
   
 SET @return = (  
  SELECT  
   ((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0)    
  FROM  
   proposal_detail_audiences pda (NOLOCK)  
   JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id  
  WHERE  
   pda.audience_id=@audience_id  
   AND pda.proposal_detail_id=@proposal_detail_id  
 )  
   
 RETURN @return  
END  

