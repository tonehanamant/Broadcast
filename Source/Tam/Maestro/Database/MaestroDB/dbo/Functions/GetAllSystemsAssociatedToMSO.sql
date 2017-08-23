

-- =============================================
-- Author:		Joseph Jacobs
-- Create date: 10/6/2009
-- Description:	
-- =============================================
-- SELECT * FROM dbo.GetAllSystemsAssociatedToMSO(1, '9/15/2009')
CREATE FUNCTION [dbo].[GetAllSystemsAssociatedToMSO]
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
			(active=1 OR active is null)
			AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
			AND system_id IN (
				SELECT system_id FROM uvw_systemzone_universe (NOLOCK) WHERE 
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
					AND zone_id IN (
						SELECT zone_id FROM uvw_zonebusiness_universe (NOLOCK) WHERE 
							(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
							AND business_id=@business_id AND type='MANAGEDBY')
				)
		UNION
		SELECT	
			id
		FROM 
			systems (NOLOCK)
		WHERE
			(active=1 OR active is null)
			AND id NOT IN (
				SELECT system_id FROM uvw_system_universe (NOLOCK) WHERE
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
					AND system_id IN (
						SELECT system_id FROM uvw_systemzone_universe (NOLOCK) WHERE 
							(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
							AND zone_id IN (
								SELECT zone_id FROM uvw_zonebusiness_universe (NOLOCK) WHERE 
									(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
									AND business_id=@business_id AND type='MANAGEDBY')
						)
				)
			AND id IN (
				SELECT id FROM systems (NOLOCK) WHERE
					id IN (
						SELECT system_id FROM system_zones (NOLOCK) WHERE 
							zone_id IN (
								SELECT zone_id FROM zone_businesses (NOLOCK) WHERE 
									business_id=@business_id AND type='MANAGEDBY'
								)
						)
				)
	RETURN;
END

