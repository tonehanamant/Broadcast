

CREATE VIEW [dbo].[uvw_system_image_sales_model_map_universe]
AS
SELECT [system_image_sales_model_map_id] = [id]
      ,[system_id]
      ,[image_id]
      ,[sales_model_id]
      ,[map_set]
      ,[start_date] = [effective_date]
      ,[end_date] = NULL
  FROM [dbo].[system_image_sales_model_maps] with(NOLOCK)
UNION ALL
SELECT [system_image_sales_model_map_id]
      ,[system_id]
      ,[image_id]
      ,[sales_model_id]
      ,[map_set]
      ,[start_date]
      ,[end_date]
  FROM [dbo].[system_image_sales_model_map_histories] with(NOLOCK)


