


CREATE PROCEDURE [dbo].[usp_REL_GetAllReleasesOrderByName]
AS

select 
      id, 
      category_id, 
      status_id, 
      name, 
      description, 
      notes, 
      release_date, 
      confirm_by_date 
from
      releases (NOLOCK)
ORDER BY
      releases.name
