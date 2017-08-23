


-- END #CRMWRC-550-Spot location is not reading 'Currently running' when appropriate



-- Fixed an issue where the new traffic schema would not find a correct start_date.
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficStartDateForRelease]  
(  
 @release_id int  
)  
AS  
BEGIN  
 SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; --same as NOLOCK  
  
 SELECT  
  MIN(ord.start_date)  
 FROM traffic_orders ord   
 WHERE  
  ord.release_id = @release_id  
  AND ord.on_financial_reports = 1

END
