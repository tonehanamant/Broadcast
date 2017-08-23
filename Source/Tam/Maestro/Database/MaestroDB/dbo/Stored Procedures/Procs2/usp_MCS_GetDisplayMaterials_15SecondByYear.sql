-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
-- usp_MCS_GetDisplayMaterials_15SecondByYear 2011
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_15SecondByYear]
	@year INT
AS
BEGIN
    SELECT 
		dm.*
	FROM
		uvw_display_materials dm 
	WHERE
		dm.length=15
		AND (YEAR(dm.date_received)=@year OR YEAR(dm.date_created)=@year)
	ORDER BY
		dm.code
END
