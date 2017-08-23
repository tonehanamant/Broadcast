CREATE Procedure [dbo].[usp_REL_GetReelsForMaterialAndRelease]
(
	@material_id as int,
	@release_id as int
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
	traffic_materials TM WITH (NOLOCK)
	join reel_materials rm WITH (NOLOCK) ON TM.reel_material_id = rm.id
	join reels R WITH (NOLOCK) on R.id = rm.reel_id
	join traffic T with (NOLOCK) on T.id = TM.traffic_id	
	join releases RL WITH (NOLOCK) on RL.id = T.release_id
where 
	rm.material_id = @material_id
	and	
	RL.id = @release_id
order by 
	R.date_created DESC
