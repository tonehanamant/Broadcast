-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetAffidavitHashesBySystemAndMonth] 
	@system_id int,
	@media_month_id int
AS
BEGIN
	SELECT 
		a.hash 
	FROM 
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
			AND i.system_id=@system_id
	WHERE 
		a.media_month_id=@media_month_id
		AND a.hash IS NOT NULL
END
