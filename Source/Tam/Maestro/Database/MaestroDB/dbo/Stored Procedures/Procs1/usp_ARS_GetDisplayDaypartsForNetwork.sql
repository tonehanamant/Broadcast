-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetDisplayDaypartsForNetwork]
	@network_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT
		id,
		code,
		name,
		start_time,
		end_time,
		mon,
		tue,
		wed,
		thu,
		fri,
		sat,
		sun
	FROM
		vw_ccc_daypart
	WHERE
		id IN (SELECT daypart_id FROM nielsen_network_rating_dayparts WHERE nielsen_network_id IN (SELECT id FROM nielsen_networks WHERE nielsen_id IN (SELECT map_value FROM network_maps WHERE map_set='Nielsen' AND network_id=@network_id)))
END
