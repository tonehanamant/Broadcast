-- =============================================
-- Author:		Stephen DeFUsco
-- Create date: 7/24/2009
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayCoverageUniverses]
AS
BEGIN
	SELECT
		cu.id,
		e.firstname + ' ' + e.lastname,
		mm.media_month,
		cu.date_approved,
		cu.date_created,
		cu.date_last_modified
	FROM
		coverage_universes cu (NOLOCK)
		JOIN media_months mm (NOLOCK) ON cu.base_media_month_id=mm.id
		LEFT JOIN employees e (NOLOCK) ON cu.approved_by_employee_id=e.id
	ORDER BY
		mm.start_date DESC
END
