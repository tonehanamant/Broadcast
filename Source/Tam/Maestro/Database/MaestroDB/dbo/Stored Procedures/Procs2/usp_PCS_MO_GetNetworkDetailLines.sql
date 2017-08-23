CREATE PROCEDURE usp_PCS_MO_GetNetworkDetailLines  
(  
 @proposal_id int  
)  
AS   
SELECT   
 DISTINCT   
 CASE WHEN D.MON = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.TUE = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.WED = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.THU = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.FRI = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.SAT = 1 THEN 'Y' ELSE 'N' END +  
 CASE WHEN D.SUN = 1 THEN 'Y' ELSE 'N' END [daypart days],  
 replace(CONVERT(VARCHAR(8), pd.start_date, 2) , '.', '') [start date],  
 replace(CONVERT(VARCHAR(8), pd.end_date, 2) , '.', '') [end date],  
 RIGHT('00'+CAST(round(d.start_time / 3600, 0) AS VARCHAR(2)),2) + RIGHT('00'+CAST(round((d.start_time) % 60, 0) AS VARCHAR(2)),2) [start_time],  
 RIGHT('00'+CAST(round(d.end_time / 3600, 0) AS VARCHAR(2)),2)  + RIGHT('00'+CAST(round((d.end_time+1) % 60, 0) AS VARCHAR(2)),2) [end_time],  
 n.name [Network Name],  
 n.code [Network Code],  
 '000' [Daypart Segment],  
 'S' [spot length type],  
 s.length [spot length],  
 pd.proposal_rate,  
 '' [supp rate code],  
 '0' [supp rate],  
 pd.num_spots [num spots],  
 0 [bb spots],   
 PD.id [proposal detail id],  
 n.network_id  
   
FROM  
 proposals p (NOLOCK)  
 join proposal_details pd (NOLOCK) on pd.proposal_id = p.id  
 join vw_ccc_daypart d (NOLOCK) on d.id = pd.daypart_id  
 join uvw_network_universe n (NOLOCK) on n.network_id = pd.network_id and (n.start_date<=pd.start_date AND (n.end_date>=pd.start_date OR n.end_date IS NULL))  
 join spot_lengths s on s.id = pd.spot_length_id  
WHERE  
 p.id = @proposal_id and pd.include = 1  
ORDER BY  
 n.code, pd.id  
