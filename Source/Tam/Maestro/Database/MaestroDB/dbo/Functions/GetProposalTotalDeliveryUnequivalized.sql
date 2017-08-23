CREATE FUNCTION [dbo].[GetProposalTotalDeliveryUnequivalized]  
(  
 @proposal_id INT,  
 @audience_id INT  
)  
RETURNS FLOAT  
AS  
BEGIN  
 DECLARE @return AS FLOAT  
   
 SET @return = (  
  SELECT  
   SUM(  
    CAST(pd.num_spots AS FLOAT)  
    *  
    (pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0  
   )  
  FROM  
   proposal_detail_audiences pda (NOLOCK)  
   JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id  
   JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id  
  WHERE  
   pda.audience_id=@audience_id  
   AND pd.proposal_id=@proposal_id  
 )  
   
 RETURN @return  
END  
