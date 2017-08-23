
CREATE PROCEDURE [dbo].[usp_REL_GetOnFinacialReportsByTrafficAndSystemID]
      @traffic_id as int,
	  @system_id as int
AS

	select 
		top 1 traffic_orders.on_financial_reports	
	from 
		traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) 
			on traffic_details.id = traffic_orders.traffic_detail_id
	where 
		traffic_orders.system_id = @system_id 
		and traffic_details.traffic_id = @traffic_id
	order by
		traffic_orders.on_financial_reports	DESC
