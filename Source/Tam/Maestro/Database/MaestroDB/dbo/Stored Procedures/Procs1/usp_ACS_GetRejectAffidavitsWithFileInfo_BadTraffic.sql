-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/21/2011
-- Description:	
-- Changes:		
-- =============================================
-- usp_ACS_GetRejectAffidavitsWithFileInfo_BadTraffic 354,16,NULL
CREATE PROCEDURE [dbo].[usp_ACS_GetRejectAffidavitsWithFileInfo_BadTraffic]
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
		8 'error_type',
		COUNT(*) 'total_errors',
		i.invoice_estimate_code 'scrubber_invalid_item_text',
		NULL 'min_affidavit_file_line',
		NULL 'max_affidavit_file_line',
		NULL 'affidavit_file_id',
		NULL 'checkin_filename',
		sn.id 'scrubber_note_id',
		sn.comment
	FROM
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		LEFT JOIN scrubber_notes sn (NOLOCK) ON sn.media_month_id=@media_month_id 
			AND sn.scrubber_error_code=8 
			AND sn.invalid_item=i.invoice_estimate_code
	WHERE
		a.media_month_id=@media_month_id
		AND a.traffic_id IS NULL
		AND i.invoice_estimate_code<>''
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)
	GROUP BY
		i.invoice_estimate_code,
		sn.id,
		sn.comment

	DROP TABLE #systems;
END
