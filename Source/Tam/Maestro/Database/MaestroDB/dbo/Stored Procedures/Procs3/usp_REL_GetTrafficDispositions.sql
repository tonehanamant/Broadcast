-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_REL_GetTrafficDispositions

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		id,
		disposition,
		active,
		sort_order 
	FROM 
		traffic_materials_disposition (NOLOCK) 
	ORDER BY
		sort_order
END
