-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/18/2011
-- Description:	
-- Changes:		1/18/2011	Added the addition of an optional note to the output.
-- =============================================
-- usp_ACS_GetRejectAffidavitsWithFileInfo_BadChannel 360,null,null
CREATE PROCEDURE [dbo].[usp_ACS_GetRejectAffidavitsWithFileInfo_BadChannel]
	@media_month_id INT,
	@business_id INT,
	@system_id INT
WITH RECOMPILE
AS
BEGIN
	DECLARE @effective_date DATETIME
	SELECT @effective_date = mm.start_date FROM media_months mm WITH (NOLOCK) WHERE mm.id=@media_month_id
	
	CREATE TABLE #systems (id INT)
	IF @business_id IS NOT NULL
	BEGIN
		INSERT INTO #systems
			SELECT DISTINCT i.system_id FROM invoices i WITH (NOLOCK) WHERE i.media_month_id=@media_month_id AND i.system_id IS NOT NULL

		DELETE FROM #systems WHERE id NOT IN (
			SELECT s.id FROM #systems s WHERE dbo.GetBusinessIdFromSystemId(s.id,@effective_date)=@business_id
		)
	END
	
	SELECT
		7 'error_type',
		COUNT(*) 'total_errors',
		'0 Subs for Zone ' + z.code + ' (' + n.code + ')' 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment
	FROM
		affidavits a WITH (NOLOCK)
		JOIN invoices i WITH (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af WITH (NOLOCK) ON af.id=i.affidavit_file_id
		JOIN uvw_network_universe n	WITH (NOLOCK) ON n.network_id=a.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
		JOIN uvw_zone_universe z	WITH (NOLOCK) ON z.zone_id=a.zone_id		  AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
		LEFT JOIN scrubber_notes sn WITH (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=7 
			AND sn.invalid_item='0 Subs for Zone ' + z.code + ' (' + n.code + ')'
	WHERE
		a.media_month_id=@media_month_id
		AND a.air_date IS NOT NULL
		AND a.zone_id IS NOT NULL
		AND a.network_id IS NOT NULL
		AND (a.subscribers IS NULL  OR a.subscribers=0)
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_copy,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment,
		z.code,
		n.code

	DROP TABLE #systems
END
