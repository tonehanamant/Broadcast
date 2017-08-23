

CREATE Procedure [dbo].[usp_TCS_GetSalesModelForTraffic]
      (
            @traffic_id Int
      )
AS

select top 1 proposal_sales_models.sales_model_id from proposal_sales_models (NOLOCK) join
traffic_proposals (NOLOCK) on proposal_sales_models.proposal_id = traffic_proposals.proposal_id
where
traffic_proposals.traffic_id = @traffic_id and traffic_proposals.primary_proposal = 1

