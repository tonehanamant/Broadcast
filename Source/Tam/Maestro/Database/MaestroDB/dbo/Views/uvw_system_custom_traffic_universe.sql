CREATE VIEW [dbo].[uvw_system_custom_traffic_universe]
AS
SELECT [system_id]
      ,[traffic_factor]
      ,[start_date] = [effective_date]
      ,[end_date] = NULL
FROM [dbo].[system_custom_traffic] with(NOLOCK)
UNION ALL
SELECT [system_id]
	   ,[traffic_factor]
	   ,[start_date]
	   ,[end_date]
FROM [dbo].[system_custom_traffic_histories] with(NOLOCK)
