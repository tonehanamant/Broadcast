
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectMaximumTopographySystemGroupDate]
	@topography_id int,
	@system_group_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT MAX(start_date) FROM uvw_topography_system_group_universe (NOLOCK) WHERE topography_id=@topography_id AND system_group_id=@system_group_id
END

