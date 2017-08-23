-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_OriginalByYear 2010
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_OriginalByYear]
	@year INT
AS
BEGIN
	SELECT
		dm.*
	FROM
		uvw_display_materials dm
	WHERE
		dm.type='Original'
		AND (YEAR(dm.date_received)=@year OR YEAR(dm.date_created)=@year)
	ORDER BY
		dm.code
END
