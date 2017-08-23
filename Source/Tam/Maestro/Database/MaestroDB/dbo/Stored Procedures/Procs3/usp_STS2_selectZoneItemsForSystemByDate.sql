

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneItemsForSystemByDate]
	@active bit,
	@system_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		zone_id,
		name + ' (' + code + ')',
		start_date 
	FROM 
		uvw_zone_universe (NOLOCK) 
	WHERE 
		zone_id IN (
			SELECT zone_id FROM uvw_systemzone_universe (NOLOCK) WHERE 
				system_id=@system_id	
				AND ((@effective_date IS NULL AND uvw_systemzone_universe.end_date IS NULL) OR (uvw_systemzone_universe.start_date<=@effective_date AND (uvw_systemzone_universe.end_date>=@effective_date OR uvw_systemzone_universe.end_date IS NULL)))
		)
		AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND (@active IS NULL OR active=@active)
	ORDER BY 
		code
END


