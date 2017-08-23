-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetSystemItemsInInvoiceMonth]
	@media_month_id INT,
	@statement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT	
		s.id,
		s.code
	FROM
		systems s (NOLOCK)
	WHERE
		s.id IN (
			SELECT DISTINCT 
				CASE WHEN i.system_id IN (698,749) THEN i.invoicing_system_id ELSE i.system_id END FROM invoices i (NOLOCK) 
			WHERE 
				i.media_month_id = @media_month_id 
				AND i.affidavit_file_id IN (
					SELECT id FROM affidavit_files af (NOLOCK) WHERE af.business_unit_id=CASE @statement_type WHEN 0 THEN 1 WHEN 1 THEN 4 WHEN 2 THEN 11 END
				)
		)

 --   SELECT
	--	DISTINCT s.id,
	--	s.code
	--FROM
	--	invoices i (NOLOCK)
	--	JOIN affidavit_files af (NOLOCK) ON af.id=i.affidavit_file_id
	--		AND af.business_unit_id=CASE @statement_type WHEN 0 THEN 1 WHEN 1 THEN 4 WHEN 2 THEN 11 END
	--	JOIN systems s (NOLOCK) ON s.id = CASE WHEN i.system_id IN (698,749) THEN i.invoicing_system_id ELSE i.system_id END
	--WHERE
	--	i.media_month_id = @media_month_id
	--ORDER BY
	--	s.code
END
