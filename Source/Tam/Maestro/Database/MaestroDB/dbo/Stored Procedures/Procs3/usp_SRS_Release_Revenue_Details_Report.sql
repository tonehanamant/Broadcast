
CREATE PROCEDURE [dbo].[usp_SRS_Release_Revenue_Details_Report] 
(
@media_month AS VARCHAR(5), @idRelease Int, @idTrafficOrders Int
)
AS

/* -----------------------------------------------------------------------------------------------

 Modifications Log:
	- object: usp_SRS_Release_Revenue_Details_Report
	- Coder : Rich Sara
	- Date  : 9/22/2010 
	- ModID : n\a 
	- Narrative for changes made: 
	-  1: Initial Creation 

 To execute: Exec usp_SRS_Missing_Iscis_Report 'nn'  -- where 'nn' = @media_month. Ex (below):
   Exec usp_SRS_Release_Revenue_Details_Report '1210',  450, 7953
   
------------------------------------------------------------------------------------------------ */

BEGIN

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

/*
	select
		n_t.code [Trafficked Network],
		s_t.code [Traffic SBT],
		z.code [Zone Code],
		n_o.code [Ordered Network],
		tro.ordered_spots [Units Ordered],
		cast(tro.ordered_spot_rate as money) [Unit Rate],
		cast((tro.ordered_spot_rate * tro.ordered_spots) as money) [Dollars Ordered]
	from
		releases r
		join traffic tr on	
			tr.release_id = r.id
		join traffic_details trd  on 
			tr.id = trd.traffic_id
		join traffic_orders tro on
			trd.id = tro.traffic_detail_id
		-- begin-rs+
	    JOIN traffic_detail_weeks tdw on tdw.traffic_detail_id = trd.id 
			and tro.start_date >= tdw.start_date and tro.end_date <= tdw.end_date
			and tdw.suspended = 0 
		-- end-rs+
		cross apply udf_GetZonesAsOf(tr.start_date) z
		cross apply udf_GetNetworksAsOf(tr.start_date) n_o
		cross apply udf_GetNetworksAsOf(tr.start_date) n_t
		cross apply udf_GetSystemsAsOf(tr.start_date) s_t 
	where
		r.id = @idRelease
		and				  
		trd.traffic_id = @idTrafficOrders 
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
	order by
		[Traffic SBT],
		[Trafficked Network],
		[Zone Code];
*/

	select
		n_t.code [Trafficked Network],
		s_t.code [Traffic SBT],
		z.code [Zone Code],
		n_o.code [Ordered Network],
		sum(tro.ordered_spots) [Units Ordered],
		sum(cast(tro.ordered_spot_rate as money)) / count(*) [Unit Rate],
		sum(cast((tro.ordered_spot_rate * tro.ordered_spots) as money)) [Dollars Ordered] 
	from
		releases r
		join traffic tr on	
			tr.release_id = r.id
		join traffic_details trd  on 
			tr.id = trd.traffic_id
		join traffic_orders tro on
			trd.id = tro.traffic_detail_id
		-- begin-rs+
	    JOIN traffic_detail_weeks tdw on tdw.traffic_detail_id = trd.id 
			and tro.start_date >= tdw.start_date and tro.end_date <= tdw.end_date
			and tdw.suspended = 0 
		-- end-rs+
		cross apply udf_GetZonesAsOf(tr.start_date) z
		cross apply udf_GetNetworksAsOf(tr.start_date) n_o
		cross apply udf_GetNetworksAsOf(tr.start_date) n_t
		cross apply udf_GetSystemsAsOf(tr.start_date) s_t 
	where
		r.id = @idRelease
		and				  
		trd.traffic_id = @idTrafficOrders 
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
	group by
		s_t.code,
		n_t.code,
		n_o.code,
		z.code,
		tro.ordered_spot_rate 
	order by
		[Traffic SBT],
		[Trafficked Network],
		[Zone Code],
		tro.ordered_spot_rate;
		
END

