﻿



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_WEB_GetOperatorStatementOfAccountReport 
CREATE PROCEDURE [dbo].[usp_WEB_GetOperatorStatementOfAccountReport]
	@media_month_id INT,
	@system_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		s.code,
		i.external_id,
		COUNT(*),
		SUM(CAST(invoice_gross_due/100.0 AS MONEY))
	FROM
		invoices i (NOLOCK)
		JOIN uvw_system_universe s ON s.system_id=i.system_id AND (s.start_date<=i.date_created AND (s.end_date>=i.date_created OR s.end_date IS NULL))
	WHERE
		i.media_month_id=@media_month_id
		AND s.system_id=@system_id
	GROUP BY
		s.code,
		i.external_id
	ORDER BY
		s.code,
		i.external_id
END




