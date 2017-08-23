-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetDisplayDaypartsForMapSet]
	@map_set VARCHAR(15)
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
		id IN (
			SELECT daypart_id FROM daypart_maps WHERE map_set=@map_set
		)
END
