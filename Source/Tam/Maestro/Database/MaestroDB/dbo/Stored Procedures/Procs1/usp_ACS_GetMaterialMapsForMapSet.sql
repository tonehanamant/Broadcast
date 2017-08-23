-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetMaterialMapsForMapSet]
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		id,
		material_id,
		map_set,
		map_value,
		active,
		effective_date
	FROM
		material_maps (NOLOCK)
	WHERE
		map_set=@map_set
END