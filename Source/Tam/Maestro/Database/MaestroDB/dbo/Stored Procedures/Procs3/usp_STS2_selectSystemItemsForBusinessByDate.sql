-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemItemsForBusinessByDate]
	@active bit,
	@business_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		system_id,
		code,
		start_date 
	FROM 
		uvw_system_universe (NOLOCK) 
	WHERE 
		system_id IN (
			SELECT system_id FROM uvw_systemzone_universe (NOLOCK) WHERE zone_id IN (
				SELECT zone_id FROM uvw_zonebusiness_universe (NOLOCK) WHERE 
					business_id=@business_id 
					AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
					AND type='OWNEDBY'
				)
				AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		) 
		AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND (@active IS NULL OR active=@active)
	ORDER BY code
END
