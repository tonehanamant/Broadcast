-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_GetNetworkMaps]
	@media_month_id INT,
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT DISTINCT 
		nm.id,
		nm.network_id,
		nm.map_set,
		nm.map_value,
		nm.active,
		nm.flag,
		nm.effective_date
	FROM 
		affidavits a (NOLOCK) 
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
		JOIN network_maps nm (NOLOCK) ON nm.map_set=(@map_set + CAST(i.system_id AS VARCHAR(31)))
	WHERE 
		a.media_month_id=@media_month_id
		AND a.network_id IS NULL
		AND a.affidavit_net=nm.map_value
		AND i.system_id IS NOT NULL
END
