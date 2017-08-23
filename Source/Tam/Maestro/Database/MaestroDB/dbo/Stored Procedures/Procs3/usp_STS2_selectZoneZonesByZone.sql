-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneZonesByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		primary_zone_id,
		secondary_zone_id,
		type,
		effective_date
	FROM
		zone_zones (NOLOCK)
	WHERE
		primary_zone_id=@zone_id
END
