	-- =============================================
	-- Author:		<Kheynis,Nick>
	-- Create date: <1/7/15>
	-- Description:	<Description,,>
	-- =============================================
	CREATE PROCEDURE [dbo].[usp_STS2_GetZonesForScrubbing] 
	AS
	BEGIN
		SET NOCOUNT ON;

		SELECT 
			z.id,
			z.code,
			z.name,
			b.name 'mso'
		FROM
			dbo.zones z (NOLOCK)
			JOIN dbo.zone_businesses zb (NOLOCK) ON zb.zone_id = z.id
			JOIN dbo.businesses b (NOLOCK) ON b.id = zb.business_id
		WHERE
			z.active = 1
			AND zb.type = 'MANAGEDBY'
	END

