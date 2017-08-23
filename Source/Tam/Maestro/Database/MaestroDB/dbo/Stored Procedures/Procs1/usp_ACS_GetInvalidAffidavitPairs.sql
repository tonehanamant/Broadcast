-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetInvalidAffidavitPairs]
AS
BEGIN
	SELECT DISTINCT 
		i.system_id, 
		i.media_month_id 
	FROM 
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) on i.id=a.invoice_id 
			AND i.system_id IS NOT NULL
	WHERE 
		a.status_id = 2 
END
