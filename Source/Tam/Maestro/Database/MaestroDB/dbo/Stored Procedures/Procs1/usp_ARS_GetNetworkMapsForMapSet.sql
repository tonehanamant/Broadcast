-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARS_GetNetworkMapsForMapSet
	@map_set VARCHAR(15)
AS
BEGIN
	SELECT
		id,
		network_id,
		map_set,
		map_value,
		active,
		flag,
		effective_date
	FROM
		network_maps
	WHERE
		map_set=@map_set
END
