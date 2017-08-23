-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectZoneDmaBusinessObjectByZone]
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		zone_dmas.zone_id,
		zone_dmas.dma_id,
		zone_dmas.weight,
		zone_dmas.effective_date,
		dmas.name,
		zones.code,
		zones.name
	FROM
		zone_dmas
		JOIN zones ON zones.id=zone_dmas.zone_id
		JOIN dmas ON dmas.id=zone_dmas.dma_id
	WHERE
		zone_dmas.zone_id=@zone_id
END
