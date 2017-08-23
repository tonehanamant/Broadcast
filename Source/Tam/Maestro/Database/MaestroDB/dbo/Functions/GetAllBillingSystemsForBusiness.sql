


-- =============================================
-- Author:		Joseph Jacobs
-- Create date: 10/6/2009
-- Description:	
-- =============================================
-- SELECT * FROM dbo.[GetAllBillingSystemsForBusiness](1, '9/15/2009')
CREATE FUNCTION [dbo].[GetAllBillingSystemsForBusiness]
(	
	@business_id INT,
	@effective_date datetime
)
RETURNS @systems TABLE
(
	system_id INT
)
AS
BEGIN
	INSERT INTO @systems

	SELECT 
		system_id
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
		system_id
RETURN;
END


