


CREATE PROCEDURE [dbo].[usp_STS2_GetCompanyLogoId]
(
      @system_id int,
	  @sales_model_id int,
	  @map_set varchar(63)
)
AS

select 
      system_image_sales_model_maps.image_id
from
      system_image_sales_model_maps with(NOLOCK)
WHERE
      system_image_sales_model_maps.system_id = @system_id
	  and system_image_sales_model_maps.sales_model_id = @sales_model_id
	  and system_image_sales_model_maps.map_set = @map_set
order by system_image_sales_model_maps.effective_date desc;


