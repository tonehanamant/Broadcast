CREATE PROCEDURE [dbo].[usp_TCS_get_all_releases]
AS
SELECT
	releases.id,
	releases.status_id,
	releases.name,
	releases.confirm_by_date,
	statuses.name AS status_name
FROM
	releases, statuses
where releases.status_id = statuses.id
