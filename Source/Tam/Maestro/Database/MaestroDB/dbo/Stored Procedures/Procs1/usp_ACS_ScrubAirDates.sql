-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_ScrubAirDates]
	@media_month_id INT,
	@business_id INT,
	@system_id INT,
	@invalid_air_date VARCHAR(31),
	@air_date DATETIME
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

    UPDATE
		affidavits
	SET
		affidavits.air_date=@air_date
	FROM
		affidavits
		JOIN invoices i (NOLOCK) ON i.id=affidavits.invoice_id
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.status_id=2
		AND affidavits.affidavit_air_date=@invalid_air_date
		AND affidavits.air_date IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)

	DROP TABLE #systems;
END
