-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneStateBusinessObjectsByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		zone_states.zone_id,
		zone_states.state_id,
		zone_states.weight,
		zone_states.effective_date,
		states.code,
		states.name,
		zones.code,
		zones.name
	FROM
		zone_states
		JOIN zones ON zones.id=zone_states.zone_id
		JOIN states ON states.id=zone_states.state_id
	WHERE
		zone_states.zone_id=@zone_id
END
