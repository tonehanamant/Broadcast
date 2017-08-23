
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_GetSystemsByMsoAndDate 8, '10/2/2009'
CREATE PROCEDURE [dbo].[usp_STS2_GetSystemsByMsoAndDate]
	@business_id INT,
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		system_id,
		code,
		name,
		location,
		spot_yield_weight,
		traffic_order_format,
		flag,
		active,
		start_date
	FROM 
		uvw_system_universe (NOLOCK) 
	WHERE 
		system_id IN (
			SELECT 
				system_id 
			FROM 
				uvw_systemzone_universe (NOLOCK) 
			WHERE 
				zone_id IN (
					SELECT 
						zone_id 
					FROM 
						uvw_zonebusiness_universe (NOLOCK) 
					WHERE 
						business_id=@business_id 
						AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
						AND type='MANAGEDBY'
						AND active=1
				)
				AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
				AND type='BILLING'
		) 
		AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND active=1
	ORDER BY 
		code
END