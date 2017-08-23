-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/18/2011
-- Description:	
-- Changes:		1/18/2011	Added the addition of an optional note to the output.
-- =============================================
-- usp_ACS_GetRejectAffidavitsWithFileInfo 360,NULL,NULL
CREATE PROCEDURE [dbo].[usp_ACS_GetRejectAffidavitsWithFileInfo]
	@media_month_id INT,
	@business_id INT,
	@system_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SELECT @effective_date = mm.start_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id

	CREATE TABLE #systems (id INT)
	IF @business_id IS NOT NULL
	BEGIN
		INSERT INTO #systems
			SELECT DISTINCT i.system_id FROM invoices i (NOLOCK) WHERE i.media_month_id=@media_month_id AND i.system_id IS NOT NULL

		DELETE FROM #systems WHERE id NOT IN (
			SELECT s.id FROM #systems s WHERE dbo.GetBusinessIdFromSystemId(s.id,@effective_date)=@business_id
		)
	END
	
	-- NETWORKS
	SELECT
		1 'error_type',
		COUNT(*) 'total_errors',
		a.affidavit_net 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		s.id,
		s.code
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN systems s (NOLOCK) ON s.id=i.system_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=1 
			AND sn.invalid_item=a.affidavit_net + '_' + CAST(s.id AS VARCHAR(MAX))
	WHERE
		a.media_month_id=@media_month_id
		AND a.network_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_net,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment,
		s.id,
		s.code
	
	UNION ALL
	
	-- MATERIALS
	SELECT
		2 'error_type',
		COUNT(*) 'total_errors',
		a.affidavit_copy 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=2 
			AND sn.invalid_item=a.affidavit_copy
	WHERE
		a.media_month_id=@media_month_id
		AND a.material_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_copy,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment
		
	UNION ALL
	
	-- AIR DATE
	SELECT
		3 'error_type',
		COUNT(*) 'total_errors',
		a.affidavit_air_date 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=3 
			AND sn.invalid_item=a.affidavit_air_date
	WHERE
		a.media_month_id=@media_month_id
		AND a.air_date IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_air_date,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment
		
	UNION ALL
		
	-- AIR TIME
	SELECT
		4 'error_type',
		COUNT(*) 'total_errors',
		a.affidavit_air_time 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=4 
			AND sn.invalid_item=a.affidavit_air_time
	WHERE
		a.media_month_id=@media_month_id
		AND a.air_time IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_air_time,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment
		
	UNION ALL
	
	-- ZONE
	SELECT
		5 'error_type',
		COUNT(*) 'total_errors',
		'Zone [' + a.affidavit_syscode + '] does not exist' 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=5 
			AND sn.invalid_item='Zone [' + a.affidavit_syscode + '] does not exist'
	WHERE
		a.media_month_id=@media_month_id
		AND a.zone_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_syscode,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment
		
	UNION ALL
	
	-- SPOT LENGTH
	SELECT
		6 'error_type',
		COUNT(*) 'total_errors',
		a.affidavit_length 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=6 
			AND sn.invalid_item=a.affidavit_length
	WHERE
		a.media_month_id=@media_month_id
		AND a.spot_length_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_length,
		a.affidavit_file_line,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment
		
	UNION ALL
		
	-- CHANNEL
	SELECT
		7 'error_type',
		COUNT(*) 'total_errors',
		'0 Subs for Zone ' + z.code + ' (' + n.code + ')' 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=a.network_id AND (n.start_date<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
		JOIN uvw_zone_universe z	(NOLOCK) ON z.zone_id=a.zone_id		  AND (z.start_date<=@effective_date AND (z.end_date>=@effective_date OR z.end_date IS NULL))
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
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
		
	UNION ALL
	
	-- INACTIVE ZONE
	SELECT
		9 'error_type',
		COUNT(*) 'total_errors',
		'Zone [' + a.affidavit_syscode + '] exists but is inactive in this media month.' 'scrubber_invalid_item_text',
		MIN(a.affidavit_file_line) 'min_affidavit_file_line',
		MAX(a.affidavit_file_line) 'max_affidavit_file_line',
		af.id 'affidavit_file_id',
		af.checkin_filename,
		sn.id 'scrubber_note_id',
		sn.comment,
		NULL,
		NULL
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
		JOIN uvw_zone_universe z (NOLOCK) ON z.code=a.affidavit_syscode 
			AND z.active=0 
			AND (z.start_date<=a.air_date AND (z.end_date>=a.air_date OR z.end_date IS NULL))
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=9 
			AND sn.invalid_item='Zone [' + a.affidavit_syscode + '] exists but is inactive in this media month.'
	WHERE
		a.media_month_id=@media_month_id
		AND a.zone_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		a.affidavit_syscode,
		af.id,
		af.checkin_filename,
		sn.id,
		sn.comment

	DROP TABLE #systems
END
