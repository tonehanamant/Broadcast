


CREATE VIEW [dbo].[uvw_system_sales_model_map_universe]
AS
SELECT [system_sales_model_map_id] = [id]
      ,[system_id]
      ,[sales_model_id]
      ,[map_set]
      ,[map_value]
      ,[start_date] = [effective_date]
      ,[end_date] = NULL
  FROM [dbo].[system_sales_model_maps] with(NOLOCK)
UNION ALL
SELECT [system_sales_model_map_id]
      ,[system_id]
      ,[sales_model_id]
      ,[map_set]
      ,[map_value]
      ,[start_date]
      ,[end_date]
  FROM .[dbo].[system_sales_model_map_histories] with(NOLOCK)


