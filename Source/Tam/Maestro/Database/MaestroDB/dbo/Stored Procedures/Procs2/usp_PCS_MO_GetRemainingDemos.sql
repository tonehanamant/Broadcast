CREATE PROCEDURE usp_PCS_MO_GetRemainingDemos  
(  
 @proposal_id int  
)  
AS   
  
SELECT   
 DISTINCT   
 'I' [Demo form],  
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
 pa.ordinal   
FROM  
 proposals p (NOLOCK)  
 join proposal_audiences pa (NOLOCK) on pa.proposal_id = p.id and pa.ordinal <> p.guarantee_type  
 join audiences a (NOLOCK) on a.id = pa.audience_id   
WHERE  
 p.id = @proposal_id and pa.audience_id <> 31  
ORDER BY  
 pa.ordinal  
 
