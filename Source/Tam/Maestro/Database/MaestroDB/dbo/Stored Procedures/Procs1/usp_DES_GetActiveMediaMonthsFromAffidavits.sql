-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_DES_GetActiveMediaMonthsFromAffidavits]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT DISTINCT
		mm.id,
		mm.[year],
		mm.[month],
		mm.media_month,
		mm.start_date,
		mm.end_date
	FROM
		invoices i (NOLOCK)
		JOIN media_months mm (NOLOCK) ON 
			mm.id = i.media_month_id
		left JOIN posted_media_months pmm (NOLOCK) ON 
			i.media_month_id = pmm.media_month_id 
	WHERE
		0 = isnull(pmm.complete, 0)
	order by
		mm.start_date;
END
