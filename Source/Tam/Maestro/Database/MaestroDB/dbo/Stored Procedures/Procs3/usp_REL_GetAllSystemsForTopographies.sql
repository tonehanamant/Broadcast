CREATE PROCEDURE [dbo].[usp_REL_GetAllSystemsForTopographies]
	@topography_id int,
	@effective_date datetime
AS

select
	system_id
from
	dbo.GetSystemsByTopographyAndDate(@topography_id, @effective_date)
order by
	system_id
