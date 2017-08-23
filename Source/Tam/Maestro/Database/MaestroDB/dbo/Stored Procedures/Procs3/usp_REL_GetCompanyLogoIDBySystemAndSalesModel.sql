
CREATE PROCEDURE [dbo].[usp_REL_GetCompanyLogoIDBySystemAndSalesModel]
(
      @system_id int,
	  @sales_model_id int,
	  @effective_date datetime
)
AS

declare @numimages int;

select @numimages = count(*) 
from
      system_image_sales_model_maps (NOLOCK)
WHERE
      system_image_sales_model_maps.system_id = @system_id
	  and system_image_sales_model_maps.sales_model_id = @sales_model_id
	  and system_image_sales_model_maps.map_set = 'release_logos' 
	  and system_image_sales_model_maps.effective_date <= @effective_date;
	  
IF (@numimages > 0)
BEGIN
select 
      top 1 system_image_sales_model_maps.image_id
from
      system_image_sales_model_maps (NOLOCK)
WHERE
      system_image_sales_model_maps.system_id = @system_id
	  and system_image_sales_model_maps.sales_model_id = @sales_model_id
	  and system_image_sales_model_maps.map_set = 'release_logos'
	and system_image_sales_model_maps.effective_date <= @effective_date
	order by system_image_sales_model_maps.effective_date desc;
END
ELSE
BEGIN
select 
      top 1 system_image_sales_model_maps.image_id
from
      system_image_sales_model_maps (NOLOCK)
WHERE
      system_image_sales_model_maps.system_id is null
	  and system_image_sales_model_maps.sales_model_id = @sales_model_id
	  and system_image_sales_model_maps.map_set = 'release_logos'
	  and system_image_sales_model_maps.effective_date <= @effective_date
	order by system_image_sales_model_maps.effective_date desc;
END
