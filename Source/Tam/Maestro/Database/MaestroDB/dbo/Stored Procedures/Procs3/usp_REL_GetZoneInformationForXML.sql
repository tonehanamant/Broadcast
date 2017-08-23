
-- Updated to take in an effective date so that the calculation for it will be done only one.
-- The effective date calculation was taking up half of the query time.
CREATE Procedure [dbo].[usp_REL_GetZoneInformationForXML]
	@zone_id INT,
	@system_id INT,
	@traffic_id INT,
	@effective_date DATETIME
AS
BEGIN
      if(@effective_date is NULL)
      BEGIN
		 select 
			@effective_date = traffic.start_date 
		 from 
			traffic (NOLOCK)
		where 
			traffic.id = @traffic_id;
      END
      
      SELECT
            z.id, 
            z.code, 
            z.name,
            dbo.GetSubscribersForZone(z.id,@effective_date,1,NULL) 'subscribers',
            tmp.start_date,
            tmp.min_traffic_order_id
      FROM (
            SELECT
                  tro.zone_id, 
                  tro.start_date,
                  MIN(tro.id) 'min_traffic_order_id'
            FROM
                  traffic_orders tro (NOLOCK)
            WHERE
                  tro.zone_id = @zone_id  
                  AND tro.system_id = @system_id  
                  AND tro.traffic_id = @traffic_id
                  AND tro.on_financial_reports = 1
                  AND tro.active = 1
            GROUP BY 
                  tro.zone_id,
                  tro.start_date
      ) tmp
      JOIN zones z (NOLOCK) ON z.id=tmp.zone_id
      ORDER BY
            tmp.start_date
END
