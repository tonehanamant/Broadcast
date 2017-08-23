
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectMaximumTopographyStateDate]
	@topography_id int,
	@state_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAX(start_date) FROM uvw_topography_state_universe (NOLOCK) WHERE topography_id=@topography_id AND state_id=@state_id
END

