

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_WEB_GetInvoiceSystems
	@media_month_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		s.system_id,
		s.code,
		CAST(SUM(invoice_gross_due)/100.0 AS MONEY)
	FROM
		invoices i (NOLOCK)
		JOIN uvw_system_universe s ON s.system_id=i.system_id AND (s.start_date<=i.date_created AND (s.end_date>=i.date_created OR s.end_date IS NULL))
	WHERE
		i.media_month_id=@media_month_id
	GROUP BY
		s.system_id,
		s.code
	ORDER BY
		s.code
END


