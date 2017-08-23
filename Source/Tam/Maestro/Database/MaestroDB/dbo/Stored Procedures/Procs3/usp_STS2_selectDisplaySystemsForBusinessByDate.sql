
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX	
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplaySystemsForBusinessByDate]
	@active bit,
	@effective_date datetime,
	@business_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
		SELECT	
			system_id,
			code,
			name,
			location,
			spot_yield_weight,
			traffic_order_format,
			flag,
			active,
			start_date,
			end_date,
			dbo.GetSubscribersForSystem(system_id,@effective_date,1,null),
			generate_traffic_alert_excel,
			one_advertiser_per_traffic_alert,
			cancel_recreate_order_traffic_alert,
			order_regeneration_traffic_alert,
			custom_traffic_system
		FROM 
			uvw_system_universe (NOLOCK)
		WHERE 
			(@active IS NULL OR active=@active)
			AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
			AND system_id IN (
				SELECT system_id FROM uvw_systemzone_universe (NOLOCK) WHERE 
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
					AND zone_id IN (
						SELECT zone_id FROM uvw_zonebusiness_universe (NOLOCK) WHERE 
							(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
							AND business_id=@business_id AND type='OWNEDBY')
				)

		UNION

		SELECT	
			id,
			code,
			name,
			location,
			spot_yield_weight,
			traffic_order_format,
			flag,
			active,
			effective_date,
			null,
			dbo.GetSubscribersForSystem(id,@effective_date,1,null),
			generate_traffic_alert_excel,
			one_advertiser_per_traffic_alert,
			cancel_recreate_order_traffic_alert,
			order_regeneration_traffic_alert,
			custom_traffic_system
		FROM 
			systems (NOLOCK)
		WHERE
			(@active IS NULL OR active=@active)
			AND id NOT IN (
				SELECT system_id FROM uvw_system_universe (NOLOCK) WHERE
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
					AND system_id IN (
						SELECT system_id FROM uvw_systemzone_universe (NOLOCK) WHERE 
							(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
							AND zone_id IN (
								SELECT zone_id FROM uvw_zonebusiness_universe (NOLOCK) WHERE 
									(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
									AND business_id=@business_id AND type='OWNEDBY')
						)
				)
			AND id IN (
				SELECT id FROM systems (NOLOCK) WHERE
					id IN (
						SELECT system_id FROM system_zones (NOLOCK) WHERE 
							zone_id IN (
								SELECT zone_id FROM zone_businesses (NOLOCK) WHERE 
									business_id=@business_id AND type='OWNEDBY'
								)
						)
				)
	)
	ORDER BY code
END


