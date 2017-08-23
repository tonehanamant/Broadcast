-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectSystemItemsForSystemGroupByDate]
	@active bit,
	@system_group_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		system_id,
		code,
		start_date 
	FROM 
		uvw_system_universe (NOLOCK) 
	WHERE 
		system_id IN (
			SELECT system_id FROM uvw_systemgroupsystem_universe (NOLOCK) WHERE 
				system_group_id=@system_group_id
				AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		) 
		AND ((@effective_date IS NULL AND end_date IS NULL) OR (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)))
		AND (@active IS NULL OR active=@active)
	ORDER BY 
		code
END
