
CREATE PROCEDURE [dbo].[usp_REL_GetTopographiesForSystem]
	@system_id int,
	@effective_date datetime
AS
 
SELECT
	topography_id
FROM
	GetTopographiesContainingSystem(@system_id, @effective_date)

