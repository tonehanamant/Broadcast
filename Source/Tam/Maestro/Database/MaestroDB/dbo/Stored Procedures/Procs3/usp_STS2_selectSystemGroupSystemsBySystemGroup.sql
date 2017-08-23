-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemGroupSystemsBySystemGroup]
	@system_group_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT
		system_group_id,
		system_id,
		effective_date
	FROM
		system_group_systems
	WHERE
		system_group_id=@system_group_id
END
