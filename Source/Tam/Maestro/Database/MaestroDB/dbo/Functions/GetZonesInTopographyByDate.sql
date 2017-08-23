
-- =============================================
-- Author:		John Carsley
-- Create date: 02/07/2013
-- Description:  this doesn't have a udf_ prefix because existing code uses it
-- =============================================
CREATE FUNCTION [dbo].[GetZonesInTopographyByDate]
(	
	@topography_id	INT,
	@effective_date DATETIME
)
RETURNS @zone_ids TABLE
(
	id INT
)
AS
BEGIN
	DECLARE @topographyIds VARCHAR(MAX)
	SET @topographyIds = CAST(@topography_id AS VARCHAR)
	
	INSERT INTO @zone_ids
		SELECT 
			id
		FROM 
			dbo.GetZonesInTopographiesByDate(@topographyIds, @effective_date);
	
	RETURN;
END
