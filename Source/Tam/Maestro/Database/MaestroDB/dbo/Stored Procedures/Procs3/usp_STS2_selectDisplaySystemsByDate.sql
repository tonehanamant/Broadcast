
/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXX	
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplaySystemsByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   DECLARE @system_ids UniqueIdTable
	INSERT INTO @system_ids
		SELECT 
			system_id 
		FROM 
			uvw_system_universe
		WHERE 
			(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
	
	DECLARE @all_systems UniqueIdTable
	INSERT INTO @all_systems
		SELECT
			*
		FROM
			@system_ids
		UNION
		SELECT 
			s.id 
		from 
			systems s
		WHERE
			s.id NOT IN (select * from @system_ids)

	DECLARE @subs_per_system TABLE (system_id INT NOT NULL, subscribers INT NULL, PRIMARY KEY CLUSTERED(system_id))
	INSERT INTO @subs_per_system
		exec dbo.GetSubscribersForSystems @all_systems,@effective_date,1,null
		
	(
		SELECT	
			su.system_id,
			code,
			name,
			location,
			spot_yield_weight,
			traffic_order_format,
			flag,
			active,
			start_date,
			end_date,
			ss.subscribers,
			su.generate_traffic_alert_excel,
			su.one_advertiser_per_traffic_alert,
			su.cancel_recreate_order_traffic_alert,
			su.order_regeneration_traffic_alert,
			su.custom_traffic_system
		FROM 
			uvw_system_universe su (NOLOCK)
			JOIN @subs_per_system ss on ss.system_id = su.system_id
		WHERE
			(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
			
		UNION ALL
		SELECT	
			s.id,
			code,
			name,
			location,
			spot_yield_weight,
			traffic_order_format,
			flag,
			active,
			effective_date,
			null,
			ss.subscribers,
			s.generate_traffic_alert_excel,
			s.one_advertiser_per_traffic_alert,
			s.cancel_recreate_order_traffic_alert,
			s.order_regeneration_traffic_alert,
			s.custom_traffic_system
		FROM 
			systems s (NOLOCK)
			JOIN @subs_per_system ss on ss.system_id = s.id
			LEFT OUTER JOIN @system_ids si on si.id = s.id
		WHERE
			si.id is null
	) 
	ORDER BY 
		code
	OPTION(RECOMPILE)

END






