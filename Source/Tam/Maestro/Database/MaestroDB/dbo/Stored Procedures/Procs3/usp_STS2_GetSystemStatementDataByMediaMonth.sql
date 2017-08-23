-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_STS2_GetSystemStatementDataByMediaMonth 374,0
CREATE PROCEDURE [dbo].[usp_STS2_GetSystemStatementDataByMediaMonth]
	@media_month_id INT,
	@statement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		s.id,
		s.code,
		s.[name]
	FROM
		dbo.systems s (NOLOCK)

	-- by system/invoice
    SELECT 
		CASE WHEN i.system_id IN (698,749) THEN i.invoicing_system_id ELSE i.system_id END 'system_id',
		i.external_id,
		CAST(SUM(i.invoice_gross_due)/100.00 AS DECIMAL(18,2))
	FROM
		dbo.invoices i (NOLOCK)
		JOIN dbo.affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
			AND af.business_unit_id=CASE @statement_type WHEN 0 THEN 1 WHEN 1 THEN 4 WHEN 2 THEN 11 END
	WHERE
		i.media_month_id = @media_month_id
		AND i.system_id IS NOT NULL
	GROUP BY 
		CASE WHEN i.system_id IN (698,749) THEN i.invoicing_system_id ELSE i.system_id END,
		i.external_id 
	ORDER BY 
		CASE WHEN i.system_id IN (698,749) THEN i.invoicing_system_id ELSE i.system_id END,
		i.external_id
END
