

CREATE PROCEDURE [dbo].[usp_REL_UpdateOnFinacialReportsByReleaseAndSystemID]
      @release_id as int,
	  @system_id as int,
	  @on_financial_reports bit
AS

	update traffic_orders set on_financial_reports= @on_financial_reports
	where 
		traffic_orders.system_id = @system_id 
		and traffic_orders.release_id = @release_id
