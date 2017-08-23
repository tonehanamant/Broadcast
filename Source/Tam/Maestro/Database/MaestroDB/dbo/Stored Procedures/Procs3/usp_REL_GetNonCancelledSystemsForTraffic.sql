

/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetNonCancelledSystemsForTraffic] ( @traffic_id INT )
AS
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
    SELECT 
      DISTINCT
            systems.id ,
            systems.code ,
            systems.name ,
			systems.location ,
            systems.spot_yield_weight ,
            systems.traffic_order_format ,
            systems.flag ,
            systems.active ,
            systems.effective_date,
			systems.generate_traffic_alert_excel,
			systems.one_advertiser_per_traffic_alert,
			systems.cancel_recreate_order_traffic_alert,
			systems.order_regeneration_traffic_alert,
			systems.custom_traffic_system    
    FROM    traffic_orders 
            JOIN systems ON systems.id = traffic_orders.system_id
            JOIN traffic_details  ON traffic_details.id = traffic_orders.traffic_detail_id
            JOIN traffic_detail_weeks ON traffic_detail_weeks.traffic_detail_id = traffic_details.id
                      AND traffic_orders.start_date >= traffic_detail_weeks.start_date
                                                  AND traffic_orders.end_date <= traffic_detail_weeks.end_date
    WHERE   traffic_details.traffic_id = @traffic_id
			AND traffic_orders.on_financial_reports = 1
            AND traffic_orders.active = 1
            AND ( traffic_detail_weeks.suspended = 0 )
    ORDER BY systems.code


