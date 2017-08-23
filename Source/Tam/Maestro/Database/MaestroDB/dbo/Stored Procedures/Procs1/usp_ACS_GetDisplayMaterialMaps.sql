-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetDisplayMaterialMaps]
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		materials.code,
		material_maps.id,
		material_maps.material_id,
		material_maps.map_set,
		material_maps.map_value,
		material_maps.active,
		material_maps.effective_date
	FROM
		material_maps	 (NOLOCK)
		JOIN materials	 (NOLOCK) ON materials.id=material_maps.material_id
	WHERE
		material_maps.map_set=@map_set
	ORDER BY
		materials.code,
		material_maps.map_value
END
