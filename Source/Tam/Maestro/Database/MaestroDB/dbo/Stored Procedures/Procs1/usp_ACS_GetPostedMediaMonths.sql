

CREATE PROCEDURE [dbo].[usp_ACS_GetPostedMediaMonths]

AS
BEGIN

	SET NOCOUNT ON;

SELECT
		media_months.id,
		media_months.[year],
		media_months.[month],
		media_months.media_month,
		media_months.start_date,
		media_months.end_date,
	CASE 
		WHEN 
			posted_media_months.complete is null
		THEN cast(0 as bit)
		ELSE posted_media_months.complete
	END
FROM
      media_months (NOLOCK)
LEFT OUTER JOIN
	  posted_media_months (NOLOCK) 
ON posted_media_months.media_month_id = media_months.id
WHERE
      media_months.id IN (
            SELECT DISTINCT media_month_id FROM invoices (NOLOCK)
      )
ORDER BY media_months.media_month

END

