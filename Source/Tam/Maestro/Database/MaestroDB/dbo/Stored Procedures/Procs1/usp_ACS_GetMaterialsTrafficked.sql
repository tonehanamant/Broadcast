

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetMaterialsTrafficked]
	@media_month_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT DISTINCT
		m.*
	FROM
		materials m (NOLOCK)
		JOIN reel_materials rm WITH (NOLOCK) on rm.material_id = m.id
		JOIN traffic_materials tm WITH (NOLOCK) ON tm.reel_material_id =rm.id
		JOIN media_months mm (NOLOCK) ON (mm.start_date <= tm.end_date AND mm.end_date >= tm.start_date)
	WHERE
		mm.id = @media_month_id
END


