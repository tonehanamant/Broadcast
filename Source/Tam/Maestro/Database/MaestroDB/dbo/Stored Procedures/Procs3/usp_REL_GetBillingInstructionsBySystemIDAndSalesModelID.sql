
CREATE PROCEDURE [dbo].[usp_REL_GetBillingInstructionsBySystemIDAndSalesModelID]
(
      @system_id int,
	  @sales_model_id int,
	  @effective_date datetime
)
AS

declare @numimages int;

select @numimages = count(*) 
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set like 'Release order billing instructions line 1' 
	  and system_sales_model_maps.effective_date <= @effective_date;
	  
IF (@numimages > 0)
BEGIN
select 
      top 1 system_sales_model_maps.map_value
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set = 'Release order billing instructions line 1'
	and system_sales_model_maps.effective_date <= @effective_date
	order by system_sales_model_maps.effective_date desc;
END 
ELSE
BEGIN 
	select 
		  top 1 system_sales_model_maps.map_value
	from
		  system_sales_model_maps (NOLOCK)
	WHERE
		  system_sales_model_maps.system_id is null
		  and system_sales_model_maps.sales_model_id = @sales_model_id
		  and system_sales_model_maps.map_set = 'Release order billing instructions line 1'
		and system_sales_model_maps.effective_date <= @effective_date
		order by system_sales_model_maps.effective_date desc;
END

select @numimages = count(*) 
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set like 'Release order billing instructions line 2' 
	  and system_sales_model_maps.effective_date <= @effective_date;
	  
IF (@numimages > 0)
BEGIN
	select 
      top 1 system_sales_model_maps.map_value
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set = 'Release order billing instructions line 2'
	and system_sales_model_maps.effective_date <= @effective_date
	order by system_sales_model_maps.effective_date desc;
END
ELSE
BEGIN 
select 
      top 1 system_sales_model_maps.map_value
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id is null
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set = 'Release order billing instructions line 2'
	and system_sales_model_maps.effective_date <= @effective_date
	order by system_sales_model_maps.effective_date desc;
END

select @numimages = count(*) 
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set like 'Release order billing instructions line 3' 
	  and system_sales_model_maps.effective_date <= @effective_date;
	  
IF (@numimages > 0)
BEGIN
	select 
      top 1 system_sales_model_maps.map_value
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id = @system_id
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set = 'Release order billing instructions line 3'
	and system_sales_model_maps.effective_date <= @effective_date
	order by system_sales_model_maps.effective_date desc;
END
ELSE
BEGIN
	select 
      top 1 system_sales_model_maps.map_value
from
      system_sales_model_maps (NOLOCK)
WHERE
      system_sales_model_maps.system_id is null
	  and system_sales_model_maps.sales_model_id = @sales_model_id
	  and system_sales_model_maps.map_set = 'Release order billing instructions line 3'
	and system_sales_model_maps.effective_date <= @effective_date
	order by system_sales_model_maps.effective_date desc;
END
