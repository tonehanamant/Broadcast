-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/20/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectInvoicingSystemZonesByZone]
	@zone_id int
AS
BEGIN
	SELECT
		sz.*
	FROM
		system_zones sz (NOLOCK)
	WHERE
		sz.zone_id=@zone_id
		AND sz.type='INVOICING'
END
