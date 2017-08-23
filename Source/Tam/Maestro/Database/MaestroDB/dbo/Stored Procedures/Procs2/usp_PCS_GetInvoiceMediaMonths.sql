-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/24/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetInvoiceMediaMonths]
AS
BEGIN
	SELECT DISTINCT
		mm.*
	FROM
		invoices i (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.id=i.media_month_id
	ORDER BY
		mm.[year] DESC,
		mm.[month] DESC
END
