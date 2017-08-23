


CREATE PROCEDURE [dbo].[usp_STS2_GetCompanyLogoIdByDate]
(
      @system_id int,
	  @sales_model_id int,
	  @map_set varchar(63),
	  @effective_date datetime
)
AS

select 
      uvw_system_image_sales_model_map_universe.image_id
from
      uvw_system_image_sales_model_map_universe WITH(NOLOCK)
WHERE
      uvw_system_image_sales_model_map_universe.system_id = @system_id
	  and uvw_system_image_sales_model_map_universe.sales_model_id = @sales_model_id
	  and uvw_system_image_sales_model_map_universe.map_set = @map_set
	  AND (uvw_system_image_sales_model_map_universe.start_date<=@effective_date AND (uvw_system_image_sales_model_map_universe.end_date>=@effective_date OR uvw_system_image_sales_model_map_universe.end_date IS NULL))


