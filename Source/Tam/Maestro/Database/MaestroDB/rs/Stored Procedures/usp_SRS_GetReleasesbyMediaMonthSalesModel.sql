
create PROCEDURE [rs].[usp_SRS_GetReleasesbyMediaMonthSalesModel]

@media_month VARCHAR(4), 
@sales_models AS VARCHAR(31) 

AS

/* -----------------------------------------------------------------------------------------------

 Modifications Log:
	- object: usp_SRS_GetReleasesbyMediaMonthSalesModel 
	- Coder : Rich Sara
	- Date  : 1/16/2011 
	- ModID : n\a 
	- Narrative for changes made: 
	-  Report shows reports by report owner and their current SDLC "state" 
	
 To execute:  
	   Exec rs.usp_SRS_GetReleasesbyMediaMonthSalesModel    '0111', '3'

------------------------------------------------------------------------------------------------ */

BEGIN

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	IF OBJECT_ID('tempdb..#temp_release_dates') IS NOT NULL DROP TABLE #temp_release_dates;
	CREATE TABLE #temp_release_dates (release_id int,start_date datetime);
	INSERT INTO #temp_release_dates (release_id,start_date)
		   SELECT 
			releases.id,
				case 
				when min(traffic_orders.start_date) is null 
				then releases.release_date 
				else min(traffic_orders.start_date) 
				end
			FROM releases 
			left join traffic_orders  on traffic_orders.release_id = releases.id 
				and traffic_orders.ordered_spots > 0 
				and traffic_orders.on_financial_reports = 1
				AND traffic_orders.active = 1 
			JOIN traffic_details tr_dtl  ON tr_dtl.id = traffic_orders.traffic_detail_id
			JOIN traffic tr ON tr.id = tr_dtl.traffic_id 
			JOIN traffic_proposals tp  ON tp.traffic_id = tr.id
			JOIN proposal_sales_models psm ON psm.proposal_id = tp.proposal_id 
				  AND sales_model_id In
					  (select cast(StringPart As Int) From dbo.SplitString('' + @sales_models + '' , ',') Where StringPart is Not Null) 
	GROUP BY
		releases.id,
		releases.release_date;

	Select distinct
		releases.id id, releases.name + ', ' + releases.description + ', Released ' +  cast(releases.release_date as varchar)
		+ ', Rls Id: ' + cast(releases.id as varchar) Label
	From 
		media_months 
		join releases  on media_months.media_month = @media_month
		join #temp_release_dates  on #temp_release_dates.release_id = releases.id
	WHERE 
		#temp_release_dates.start_date >= media_months.start_date
		and
		#temp_release_dates.start_date <= media_months.end_date
	ORDER BY
		releases.name + ', ' + releases.description + ', Released ' +  cast(releases.release_date as varchar)
			+ ', Rls Id: ' + cast(releases.id as varchar)
	
	Drop table #temp_release_dates;

END
