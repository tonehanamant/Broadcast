

CREATE PROCEDURE [dbo].[usp_REL_GetReleasesByDate]
	 @date as datetime
AS

select id, category_id, status_id, name, description, notes, release_date, confirm_by_date from
releases (NOLOCK)
where release_date >= @date

