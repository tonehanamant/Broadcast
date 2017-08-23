
-- =============================================  
-- Author:  Nicholas Kheynis  
-- Create date: 5/22/2014  
-- Description:   
-- =============================================  
-- usp_PCS_PostSearchResults null, 2201, 'US Navy', null, null  
CREATE PROCEDURE [dbo].[usp_PCS_PostSearchResults]  
 @post_id INT,  
 @employee_id INT,  
 @search_text VARCHAR(255),  
 @start_date DATETIME,  
 @end_date DATETIME,
 @campaign_id INT  
AS  
BEGIN  
   
 IF @post_id IS NOT NULL   
 BEGIN  
  SELECT DISTINCT  
   dp.*  
  FROM   
   uvw_display_posts dp   
   JOIN tam_post_proposals tpp(NOLOCK) ON tpp.tam_post_id = dp.id   
    AND tpp.post_source_code = 0  
  WHERE   
   dp.id=@post_id  
   AND dp.sales_model_id IN (  
    SELECT sales_model_id FROM dbo.GetSalesModelsForEmployee (@employee_id)  
   )   
  ORDER BY   
   dp.id DESC  
     
 END  
   
 ELSE IF (@campaign_id IS NOT NULL)
 BEGIN
	SELECT DISTINCT  
   dp.*  
  FROM   
   uvw_display_posts dp   
   JOIN tam_post_proposals tpp(NOLOCK) ON tpp.tam_post_id = dp.id   
    AND tpp.post_source_code = 0
    JOIN tam_posts p (NOLOCK) ON dp.id = p.id  
  WHERE   
   p.campaign_id=@campaign_id  
   AND dp.sales_model_id IN (  
    SELECT sales_model_id FROM dbo.GetSalesModelsForEmployee (@employee_id)  
   )   
  ORDER BY   
   dp.id DESC  
 END
 
 ELSE
   
 BEGIN  
  SELECT DISTINCT  
   dp.*  
  FROM   
   uvw_display_posts dp   
   JOIN tam_post_proposals tpp(NOLOCK) ON tpp.tam_post_id = dp.id   
    AND tpp.post_source_code = 0  
  WHERE  
   (@search_text IS NULL OR (dp.title LIKE @search_text OR dp.post_setup_agency LIKE @search_text OR dp.post_setup_advertiser LIKE @search_text OR dp.post_setup_product LIKE @search_text))  
   AND (@start_date IS NULL OR dp.start_date >= @start_date)  
   AND (@end_date IS NULL OR dp.end_date <= @end_date)  
   AND dp.sales_model_id IN (  
    SELECT sales_model_id FROM dbo.GetSalesModelsForEmployee (@employee_id)  
   )  
  ORDER BY   
   dp.id DESC  
 END  
END
