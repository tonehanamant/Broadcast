	CREATE PROCEDURE [dbo].[usp_TCS_GetProposalsByIDWithoutTraffic]  
	 @proposal_id INT  
	AS  
	BEGIN  
	 SELECT  
	  dp.id,  
	  dp.version_number,  
	  dp.total_gross_cost,  
	  dp.advertiser,  
	  dp.product,  
	  dp.agency,  
	  dp.title,  
	  dp.salesperson,  
	  dp.flight_text,  
	  dp.include_on_availability_planner,  
	  dp.date_created,  
	  dp.date_last_modified,  
	  ps.name 'proposal_status',  
	  dp.length,  
	  rct.name 'rate_card_type',  
	  ISNULL(dp.original_proposal_id, dp.id)  
	 FROM  
	  uvw_display_proposals dp  
	  JOIN proposal_statuses ps (NOLOCK) ON ps.id=dp.proposal_status_id  
	  JOIN rate_card_types rct (NOLOCK) ON rct.id=dp.rate_card_type_id  
	  LEFT OUTER JOIN traffic_proposals tp1 ON dp.id = tp1.proposal_id
	  LEFT OUTER JOIN traffic_proposals tp2 ON dp.original_proposal_id = tp1.proposal_id
	 WHERE  
	  dp.sales_model_id <> 4   
	  AND dp.proposal_status_id NOT IN (5,6,7) -- previously ordered, cancelled before start, posting plan  
	  AND   
	  (  
	   dp.original_proposal_id=@proposal_id  
	   OR  
	   dp.id=@proposal_id  
	  )  
	  AND tp1.proposal_id IS NULL
	  AND tp2.proposal_id IS NULL
	   
	END  
	-- End bug 7462






-- Start issue 8315
/****** Object:  StoredProcedure [dbo].[usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment_v2]    Script Date: 01/22/2015 13:07:02 ******/
SET ANSI_NULLS ON
