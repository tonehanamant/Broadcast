



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplaySystemGroupsByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
		SELECT	
			system_group_id,
			name,
			active,
			flag,
			start_date,
			end_date,
			dbo.GetSubscribersForSystemGroup(system_group_id,@effective_date,1,null)
		FROM 
			uvw_systemgroup_universe (NOLOCK)
		WHERE 
			(@active IS NULL OR active=@active)
			AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))

		UNION

		SELECT	
			id,
			name,
			active,
			flag,
			effective_date,
			null,
			dbo.GetSubscribersForSystemGroup(id,@effective_date,1,null)
		FROM 
			system_groups (NOLOCK)
		WHERE 
			(@active IS NULL OR active=@active)
			AND system_groups.id NOT IN (
				SELECT system_group_id FROM uvw_systemgroup_universe (NOLOCK) WHERE 
					(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
			)
		)
		ORDER BY 
			name
END




