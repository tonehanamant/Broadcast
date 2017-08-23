
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectMaximumTopographyZoneDate]
	@topography_id int,
	@zone_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAX(start_date) FROM uvw_topography_zone_universe (NOLOCK) WHERE topography_id=@topography_id AND zone_id=@zone_id
END

