
CREATE Procedure [dbo].[usp_REL_GetReelsForMaterial]
(
	@material_id as int
)
AS

select 
	distinct R.id, 
	R.name, 
	R.description, 
	R.status_code, 
	R.has_screener,
	R.date_finalized, 
	R.date_created,
	R.date_last_modified	 
from 
	reel_materials rm with (NOLOCK)
	join reels R (NOLOCK) on R.id = rm.reel_id
where 
	rm.material_id = @material_id
order by 
	R.date_created DESC
