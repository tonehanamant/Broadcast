


/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** 03/07/2017	David Chen
** 03/13/2017	Abdul Sukkur	Added new columns to systems 
** 05/11/2017	Abdul Sukkur	Added column custom_traffic_system to systems 
*****************************************************************************************************/
-- usp_STS2_selectDisplaySystemsForSystemGroupByDate 1, '5/5/2009',1
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplaySystemsForSystemGroupByDate]
	@active bit,
	@effective_date datetime,
	@system_group_id int
AS
BEGIN
	 SET NOCOUNT ON 
	-- added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;
	--DECLARE @active bit = 1,
	--@effective_date datetime = '5/5/2009',
	--@system_group_id int = 1

	DECLARE @system_ids UniqueIdTable
	INSERT INTO @system_ids
		SELECT 
			system_id 
		FROM 
			uvw_systemgroupsystem_universe (NOLOCK)
		WHERE 
			start_date<=@effective_date 
			AND (end_date>=@effective_date OR end_date IS NULL)
			AND system_group_id=@system_group_id
		
	DECLARE @all_systems UniqueIdTable
	INSERT INTO @all_systems
		SELECT * from @system_ids
		UNION
		SELECT system_id FROM system_group_systems (NOLOCK) WHERE system_group_id=@system_group_id

	DECLARE @subs_per_system TABLE (system_id INT NOT NULL, subscribers INT NULL, PRIMARY KEY CLUSTERED(system_id))
	INSERT INTO @subs_per_system
		exec dbo.GetSubscribersForSystems @all_systems,@effective_date,1,null

	(SELECT 
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
		join @system_ids rs on rs.id = su.system_id
		left outer join @subs_per_system ss on ss.system_id = su.system_id
	WHERE 
		(@active IS NULL OR active=@active)
		AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
	UNION
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
		ssg.subscribers,
		s.generate_traffic_alert_excel,
		s.one_advertiser_per_traffic_alert,
		s.cancel_recreate_order_traffic_alert,
		s.order_regeneration_traffic_alert,
		s.custom_traffic_system
	FROM 
		systems s (NOLOCK)
		join @subs_per_system ssg on ssg.system_id = s.id
		LEFT OUTER JOIN @system_ids si on si.id = s.id
	WHERE
		(@active IS NULL OR active=@active)
		AND si.id is NULL
		)
	ORDER BY code
	OPTION(RECOMPILE)
END


