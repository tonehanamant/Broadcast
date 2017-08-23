-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectMaximumSystemZoneDate]
	@system_id int,
	@zone_id int,
	@type varchar(15)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAX(start_date) FROM uvw_systemzone_universe (NOLOCK) WHERE system_id=@system_id AND zone_id=@zone_id AND type=@type
END
