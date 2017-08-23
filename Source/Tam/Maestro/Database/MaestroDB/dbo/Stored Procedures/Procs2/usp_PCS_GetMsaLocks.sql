	CREATE PROCEDURE [dbo].[usp_PCS_GetMsaLocks]
	AS
	BEGIN
		SET NOCOUNT ON;
	
		SELECT
			mm.*,
			CASE WHEN ml.is_locked IS NULL THEN CAST(0 AS BIT) ELSE ml.is_locked END
		FROM
			media_months mm (NOLOCK)
			LEFT OUTER JOIN msa_locks ml (NOLOCK) ON ml.media_month_id=mm.id
		WHERE
			mm.id IN (
				SELECT p.posting_media_month_id FROM proposals p (NOLOCK)
			)
		ORDER BY 
			mm.start_date DESC
	END
