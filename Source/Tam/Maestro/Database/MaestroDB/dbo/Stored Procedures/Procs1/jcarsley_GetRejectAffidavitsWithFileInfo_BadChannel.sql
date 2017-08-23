CREATE PROCEDURE [dbo].[jcarsley_GetRejectAffidavitsWithFileInfo_BadChannel]
	@media_month_id INT,
	@business_id INT,
	@system_id INT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

	DECLARE @effective_date DATETIME
	SELECT 
		@effective_date = mm.start_date 
	FROM 
		media_months mm 
	WHERE 
		mm.id=@media_month_id
	
	DECLARE @systems as TABLE(id INT)
	IF @business_id IS NOT NULL
	BEGIN
		INSERT INTO @systems
			SELECT DISTINCT 
				i.system_id 
			FROM 
				invoices i  
			WHERE i.media_month_id = @media_month_id 
			AND i.system_id IS NOT NULL

		DELETE FROM @systems WHERE id NOT IN (
			SELECT 
				s.id 
			FROM 
				@systems s 
			WHERE dbo.GetBusinessIdFromSystemId(s.id,@effective_date) = @business_id
		)
	END
	
	CREATE TABLE #invoice_affidavit_file
	(
		invoice_id int NOT NULL,
		affidavit_file_id int NOT NULL,
		checkin_filename varchar(255) NOT NULL,
		system_id int NULL
	)
	CREATE NONCLUSTERED INDEX IX_invoice_id ON #invoice_affidavit_file(invoice_id)
	
	INSERT INTO #invoice_affidavit_file
	  SELECT 
		invoice_id = i.id, 
		affidavit_file_id = af.id,
		af.checkin_filename,
		i.system_id
	  FROM 
		invoices i
		JOIN affidavit_files af ON af.id=i.affidavit_file_id
	  WHERE 
		media_month_id = @media_month_id
		
	  SELECT
		error_type = 7,
		total_errors = COUNT(*),
		scrubber_invalid_item_text = '0 Subs for Zone ' + z.code + ' (' + n.code + ')',
		min_affidavit_file_line = MIN(a.affidavit_file_line),
		max_affidavit_file_line = MAX(a.affidavit_file_line),
		aif.affidavit_file_id,
		aif.checkin_filename,
		scrubber_note_id = sn.id,
		sn.comment
	 FROM
		affidavits a 
		JOIN #invoice_affidavit_file aif ON a.invoice_id = aif.invoice_id
		JOIN uvw_network_universe n ON n.network_id = a.network_id 
			AND (n.start_date <= @effective_date AND (n.end_date >= @effective_date OR n.end_date IS NULL))
		JOIN uvw_zone_universe z ON z.zone_id = a.zone_id 
			AND (z.start_date <= @effective_date AND (z.end_date >= @effective_date OR z.end_date IS NULL))
		LEFT JOIN scrubber_notes sn ON sn.media_month_id = @media_month_id 
			AND sn.scrubber_error_code = 7 
			AND sn.invalid_item = '0 Subs for Zone ' + z.code + ' (' + n.code + ')'
	WHERE
		a.media_month_id = @media_month_id
		AND a.air_date IS NOT NULL
		AND a.zone_id IS NOT NULL
		AND a.network_id IS NOT NULL
		AND COALESCE(a.subscribers,0) = 0
		AND (@business_id IS NULL 
			OR aif.system_id IN (SELECT id FROM @systems))
		AND (@system_id IS NULL	
			OR aif.system_id=@system_id)
	GROUP BY
		a.affidavit_copy,
		aif.affidavit_file_id,
		aif.checkin_filename,
		sn.id,
		sn.comment,
		z.code,
		n.code	
		--don't drop temp tables at the end of stored procs.  They will be dropped when execution ends.  
		--SQL Server handles this more efficiently
END