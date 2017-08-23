
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX			XXX
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplaySystemByDate]
	@system_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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
		(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
		AND system_id=@system_id

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
		systems.id NOT IN (
			SELECT system_id FROM uvw_system_universe (NOLOCK) WHERE 
				(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
				AND system_id=@system_id
		)
		AND systems.id=@system_id
END






