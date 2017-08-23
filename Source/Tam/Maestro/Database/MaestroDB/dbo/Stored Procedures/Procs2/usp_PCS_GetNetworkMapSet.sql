-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetNetworkMapSet
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
		network_maps (NOLOCK)
	WHERE
		map_set=@map_set
END
