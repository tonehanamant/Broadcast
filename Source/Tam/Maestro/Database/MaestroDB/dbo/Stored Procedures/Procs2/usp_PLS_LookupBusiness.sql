
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/2/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PLS_LookupBusiness]
	@zone_id INT,
	@effective_date DATETIME
AS
BEGIN
	SELECT
		b.*
	FROM
		dbo.uvw_zonebusiness_universe zb
		JOIN dbo.businesses b (NOLOCK) ON b.id=zb.business_id
	WHERE
		zb.zone_id=@zone_id
		AND zb.type='MANAGEDBY'
		AND zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL)
END

