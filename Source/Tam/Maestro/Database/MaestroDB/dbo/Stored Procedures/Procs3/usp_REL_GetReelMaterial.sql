
CREATE PROCEDURE [dbo].[usp_REL_GetReelMaterial]
    @reel_id int,
	@material_id int
AS

select
	rm.*
from
	reel_materials rm WITH (NOLOCK)
WHERE
	rm.reel_id = @reel_id and
	rm.material_id = @material_id

