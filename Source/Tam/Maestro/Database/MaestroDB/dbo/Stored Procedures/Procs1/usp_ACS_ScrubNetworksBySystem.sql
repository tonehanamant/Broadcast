-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_ScrubNetworksBySystem]
	@media_month_id INT,
	@business_id INT,
	@system_id INT,
	@invalid_network VARCHAR(15),
	@network_id INT
AS
BEGIN
	DECLARE @effective_date DATETIME
	SELECT @effective_date = mm.start_date FROM media_months mm (NOLOCK) WHERE mm.id=@media_month_id
	
	CREATE TABLE #daypart_networks (daypart_network_id INT, network_id INT, daypart_id INT, code VARCHAR(15), name VARCHAR(63), start_time INT, end_time INT, mon BIT, tue BIT, wed BIT, thu BIT, fri BIT, sat BIT, sun BIT)
	INSERT INTO #daypart_networks
		EXEC dbo.usp_PCS_GetDaypartNetworkDetailsByDate @effective_date

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
		affidavits.network_id=CASE WHEN dn.daypart_network_id IS NOT NULL THEN dn.daypart_network_id ELSE @network_id END
	FROM
		affidavits
		JOIN invoices i (NOLOCK) ON i.id=affidavits.invoice_id
		-- handles scrubbing of daypart nteworks
		LEFT JOIN #daypart_networks dn ON dn.network_id=@network_id 
			AND 1=(
				CASE WHEN dn.end_time < dn.start_time THEN 
					CASE WHEN affidavits.air_time BETWEEN dn.start_time AND 86400 OR affidavits.air_time BETWEEN 0 AND dn.end_time THEN 1 ELSE 0 END
				ELSE
					CASE WHEN affidavits.air_time BETWEEN dn.start_time AND dn.end_time THEN 1 ELSE 0 END
				END
			)
			AND 1=(
				CASE DATEPART(weekday,affidavits.air_date) WHEN 1 THEN dn.sun WHEN 2 THEN dn.mon WHEN 3 THEN dn.tue WHEN 4 THEN dn.wed WHEN 5 THEN dn.thu WHEN 6 THEN dn.fri WHEN 7 THEN dn.sat END
			)
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.status_id=2
		AND affidavit_net=@invalid_network
		AND affidavits.network_id IS NULL
		AND (@business_id IS NULL	OR i.system_id IN (SELECT id FROM #systems))
		AND (@system_id IS NULL		OR i.system_id=@system_id)

	DROP TABLE #systems;
END
