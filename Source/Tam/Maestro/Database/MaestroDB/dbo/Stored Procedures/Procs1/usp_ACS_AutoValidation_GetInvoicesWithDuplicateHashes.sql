-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
-- usp_ACS_AutoValidation_GetInvoicesWithDuplicateHashes 383
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetInvoicesWithDuplicateHashes]
	@media_month_id INT
AS
BEGIN
	-- create table of hashes havining intances > 1
	CREATE TABLE #hashes ([hash] CHAR(59), num_instances INT)
	CREATE INDEX IX_TempTable on #hashes ([hash])
	INSERT INTO #hashes
		SELECT 
			a.hash, 
			COUNT(1) 'cnt' 
		FROM 
			affidavits a(NOLOCK) 
		WHERE	
			a.media_month_id=@media_month_id
			AND a.hash IS NOT NULL
		GROUP BY 
			a.hash 
		HAVING 
			COUNT(1)>1

	-- get affidavit info with duplicate hashes
	SELECT DISTINCT
		a.invoice_id,
		a.hash 
	FROM 
		affidavits a (NOLOCK) 
		JOIN #hashes ON #hashes.hash=a.hash
	WHERE 
		a.media_month_id=@media_month_id
	ORDER BY
		a.hash

	DROP TABLE #hashes;
END
