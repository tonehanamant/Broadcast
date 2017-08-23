
/**************************************************************************************************
BEGIN:	CRMWTRF-1535 Married wizard Married Traffic report needs to show impressions from Mvpd Traffic	
**************************************************************************************************/		
CREATE FUNCTION [dbo].[GetTotalDeliveryForTrafficByWeekAndAudience]
(
       @traffic_id INT,
    @audience_id INT
)
RETURNS @delivery TABLE
(
       traffic_id INT,
       start_date DATETIME,
       delivery FLOAT
)
AS
BEGIN
       INSERT INTO @delivery (traffic_id, start_date, delivery)
              SELECT 
                     t.id, 
                     tdw.start_date, 
                     SUM(d.delivery) 'delivery'
              FROM 
                     traffic t (NOLOCK) 
                     JOIN traffic_details td (NOLOCK) ON td.traffic_id = t.id 
                     JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id = td.id
                     CROSS APPLY dbo.GetTotalDeliveryForTrafficDetailAndWeekByAudience(td.id, @audience_id) d   
              WHERE
                       t.id = @traffic_id 
                       AND tdw.id = d.traffic_detail_week_id 
                       AND tdw.suspended = 0
              GROUP BY
                       t.id, 
                       tdw.start_date

	INSERT INTO @delivery (traffic_id, start_date, delivery)
              SELECT 
                     t.id, 
                     mw.start_date, 
                     SUM(tst.spots * ((tsta.impressions_per_spot) / 1000.0)) 'delivery'
              FROM 
                     traffic t (NOLOCK) 
                     JOIN traffic_details td (NOLOCK) ON td.traffic_id = t.id 
                     JOIN traffic_spot_target_allocation_group tstag (NOLOCK) on tstag.traffic_detail_id = td.id 
                     JOIN traffic_spot_targets tst (NOLOCK) on tst.traffic_spot_target_allocation_group_id = tstag.id 
                     JOIN traffic_spot_target_audiences tsta (NOLOCK) on tsta.traffic_spot_target_id = tst.id and tsta.audience_id =  @audience_id 
                     JOIN media_weeks mw (NOLOCK) on mw.id = tstag.media_week_id 
              WHERE
                       t.id = @traffic_id 
                       and tst.suspended = 0
                       and t.plan_type = 1
              GROUP BY
                       t.id, 
                       mw.start_date
       RETURN;
END
