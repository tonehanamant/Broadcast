-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Modified:	1/18/2011
-- Description:	
-- Changes:		1/18/2011	Added the addition of an optional note to the output.
-- =============================================
-- usp_ACS_GetRejectAffidavitsBySystemWithFileInfo_BadNetwork 360,null,null
CREATE PROCEDURE [dbo].[usp_ACS_GetRejectAffidavitsBySystemWithFileInfo_BadNetwork]
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
		
	DROP TABLE #systems;
END
