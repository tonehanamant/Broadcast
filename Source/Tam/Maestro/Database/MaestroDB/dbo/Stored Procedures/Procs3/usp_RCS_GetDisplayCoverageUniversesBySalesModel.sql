
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayCoverageUniversesBySalesModel]
	@sales_model_id INT
AS
BEGIN
	SELECT
		cu.id,
		cu.approved_by_employee_id,
		mm.media_month,
		cu.date_approved,
		cu.date_created,
		cu.date_last_modified
	FROM
		coverage_universes cu (NOLOCK)
		JOIN media_months mm (NOLOCK) ON cu.base_media_month_id=mm.id
	WHERE
		sales_model_id=@sales_model_id
	ORDER BY
		mm.start_date DESC
END
