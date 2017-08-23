
CREATE PROCEDURE [dbo].[usp_SRS_Release_Revenue_Summary_Report] 
(
@media_month AS VARCHAR(5), @idRelease Int, @idTrafficOrders VARCHAR(MAX), @sales_models AS INT
)
AS

/* -----------------------------------------------------------------------------------------------

 Modifications Log:
	- object: usp_SRS_Release_Revenue_Summary_Report
	- Coder : Rich Sara
	- Date  : 9/22/2010 
	- ModID : n\a 
	- Narrative for changes made: 
	-  1: Initial Creation 

	- ModID : rs01 
	- Narrative for changes made: 
	-  1: added sales model row selection logic\relationships			

 To execute: 
   Exec usp_SRS_Release_Revenue_Summary_Report '0111',  502, 8688, 3
   
------------------------------------------------------------------------------------------------ */

BEGIN

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	select
        trd.traffic_id pid,
        case
        when len(rtrim(ltrim(tr.display_name))) = 0 then tr.name +  ', Id: ' + cast(trd.traffic_id as varchar) 
        else tr.display_name +  ', Id: ' + cast(trd.traffic_id as varchar) 
        end [TrafficOrders],
		s_t.code [Traffic SBT],
		n_t.code [Trafficked Network],
		sum(cast((tro.ordered_spot_rate * tro.ordered_spots) as money)) [Dollars Ordered]
	from
		releases r
		join traffic tr on	
			tr.release_id = r.id
		join traffic_details trd  on 
			tr.id = trd.traffic_id
		join traffic_orders tro on
			trd.id = tro.traffic_detail_id
		-- rs01 begin - add sales model selection			
		JOIN traffic_proposals tp  ON tp.traffic_id = trd.traffic_id
		JOIN proposal_sales_models psm ON psm.proposal_id =tp.proposal_id 
			and psm.sales_model_id=@sales_models
		-- rs01 end - add sales model selection			
		-- begin-ADDED traffic_detail_weeks code as per JJ directions +
	    JOIN traffic_detail_weeks tdw on tdw.traffic_detail_id = trd.id 
			and tro.start_date >= tdw.start_date and tro.end_date <= tdw.end_date
			and tdw.suspended = 0 
		-- end-ADDED traffic_detail_weeks code
		cross apply udf_GetZonesAsOf(tr.start_date) z
		cross apply udf_GetNetworksAsOf(tr.start_date) n_o
		cross apply udf_GetNetworksAsOf(tr.start_date) n_t
		cross apply udf_GetSystemsAsOf(tr.start_date) s_t 
	where
		r.id = @idRelease
		and				  
		trd.traffic_id in (select cast(StringPart As Int) 
							From dbo.SplitString('' + @idTrafficOrders + '' , ',') Where StringPart is Not Null) 
		and
		z.zone_id = tro.zone_id
		and
		n_o.network_id = tro.display_network_id
		and
		n_t.network_id = trd.network_id
		and
		s_t.system_id = tro.system_id
		and
		tro.on_financial_reports = 1
		and
		tro.active = 1
	Group by
		tr.display_name,
		tr.name,
        trd.traffic_id ,
        case
        when len(rtrim(ltrim(tr.display_name))) = 0 then tr.name +  ', Id: ' + cast(trd.traffic_id as varchar) 
        else tr.display_name +  ', Id: ' + cast(trd.traffic_id as varchar) 
        end ,
		s_t.code ,
		n_t.code
	Having 	sum(cast((tro.ordered_spot_rate * tro.ordered_spots) as money)) > 0
	order by
		tr.display_name,
		tr.name,
		n_t.code 
		
END

