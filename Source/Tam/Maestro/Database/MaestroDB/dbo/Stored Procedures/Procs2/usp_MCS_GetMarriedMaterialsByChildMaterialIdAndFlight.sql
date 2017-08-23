
-- =============================================
-- Author:		Stephen DeFUsco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMarriedMaterialsByChildMaterialIdAndFlight]
	@material_id INT,
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		m.*
	FROM
		materials m (NOLOCK)
	WHERE
		m.id IN (
			SELECT 
				original_material_id 
			FROM 
				material_revisions mr WITH (NOLOCK) 
				JOIN reel_materials rm WITH (NOLOCK) ON rm.material_id=mr.original_material_id
				JOIN traffic_materials tm WITH (NOLOCK) ON tm.reel_material_id=rm.id AND (tm.start_date <= @end_date AND tm.end_date >= @start_date)
			WHERE 
				mr.revised_material_id = @material_id
		)
	ORDER BY
		code
END

