-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetNetworkMapsForMapSet]
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
		map_set LIKE @map_set
END