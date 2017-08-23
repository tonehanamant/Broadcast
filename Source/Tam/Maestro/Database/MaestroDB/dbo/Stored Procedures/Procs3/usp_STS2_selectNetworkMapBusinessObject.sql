-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectNetworkMapBusinessObject]
	@network_id INT,
	@map_set VARCHAR(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		network_maps.id,
		network_maps.network_id,
		network_maps.map_set,
		network_maps.map_value,
		network_maps.active,
		network_maps.flag,
		network_maps.effective_date
	FROM
		network_maps
	WHERE
		network_maps.network_id=@network_id
		AND network_maps.map_set=@map_set
END
