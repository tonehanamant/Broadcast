-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetDisplayStatements]
	@statement_type TINYINT,
	@year INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	CREATE TABLE #active_invoice_month (media_month_id INT)
	INSERT INTO #active_invoice_month
		SELECT DISTINCT i.media_month_id FROM invoices i (NOLOCK)

    SELECT
		s.id,
		mm.media_month + CASE WHEN aim.media_month_id IS NULL THEN ' (the invoices for this month have been archived, you can view system statements sent, but DO NOT regenerate them)' ELSE '' END
	FROM
		statements s (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=s.media_month_id
			AND mm.[year]=@year
		LEFT JOIN #active_invoice_month aim ON aim.media_month_id=mm.id
	WHERE
		s.statement_type = @statement_type

	DROP TABLE #active_invoice_month;
END
