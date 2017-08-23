-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemGroupSystemBusinessObjectsBySystemGroup]
	@system_group_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	system_group_systems.system_group_id,
			system_group_systems.system_id,
			system_group_systems.effective_date,
			systems.code
	FROM system_group_systems
	JOIN systems ON systems.id=system_group_systems.system_id
	WHERE system_group_id=@system_group_id
	ORDER BY name
END
