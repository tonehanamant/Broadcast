-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetMaterialMapHistoriesForMapSet]
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		material_map_id,
		start_date,
		material_id,
		map_set,
		map_value,
		active,
		end_date
	FROM
		material_map_histories (NOLOCK)
	WHERE
		map_set=@map_set
END
SET ANSI_NULLS ON
