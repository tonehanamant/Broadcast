CREATE PROCEDURE [dbo].[usp_ACS_GetMediaMonths]
AS
BEGIN
	DECLARE @max_media_month_id INT;
	SELECT @max_media_month_id = MAX(i.media_month_id) FROM invoices i (NOLOCK);
	
	DECLARE @media_month_id_after_max INT;
	SELECT @media_month_id_after_max = dbo.udf_CalculateFutureMediaMonthId(@max_media_month_id, 1);

	SELECT
		mm.*
	FROM
		media_months mm (NOLOCK)
	WHERE
		mm.id IN (
			SELECT DISTINCT media_month_id FROM invoices (NOLOCK)
		)
		OR mm.id=@media_month_id_after_max
	ORDER BY
		mm.start_date DESC
END
