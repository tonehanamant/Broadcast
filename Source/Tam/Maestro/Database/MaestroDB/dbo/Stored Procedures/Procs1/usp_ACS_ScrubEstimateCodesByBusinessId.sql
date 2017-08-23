-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/8/2011
-- Description:	
-- =============================================
-- usp_ACS_ScrubEstimateCodesByBusinessId 356,8,'32257 | 32257',10203
CREATE PROCEDURE [dbo].[usp_ACS_ScrubEstimateCodesByBusinessId]
	@media_month_id INT,
	@business_id INT,
	@invalid_estimate_code VARCHAR(127),
	@traffic_id INT
AS
BEGIN
	CREATE TABLE #invoices (id INT, invoice_estimate_code VARCHAR(255))
	INSERT INTO #invoices
		SELECT
			id,
			invoice_estimate_code
		FROM
			invoices i (NOLOCK)
		WHERE
			i.media_month_id=@media_month_id
			AND dbo.GetBusinessIdFromSystemId(i.system_id,i.invoice_con_start_date)=@business_id
			AND @invalid_estimate_code = 
				CASE 
					WHEN LEN(i.invoice_estimate_code) > LEN(@invalid_estimate_code) THEN 
						SUBSTRING(i.invoice_estimate_code,1,LEN(@invalid_estimate_code))	
					ELSE
						i.invoice_estimate_code
				END
				
    UPDATE
		affidavits
	SET
		traffic_id=@traffic_id
	FROM
		affidavits
		JOIN #invoices i (NOLOCK) ON i.id=affidavits.invoice_id
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.traffic_id IS NULL
		
	DROP TABLE #invoices;
END
