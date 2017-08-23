-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/17/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PLS_LookupSystemZone]
	@zone_id INT,
	@effective_date DATETIME
AS
BEGIN
	SELECT
		s.*
	FROM
		dbo.uvw_systemzone_universe sz
		JOIN dbo.systems s (NOLOCK) ON s.id=sz.system_id
	WHERE
		sz.zone_id=@zone_id
		AND sz.type='BILLING'
		AND sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)
END


