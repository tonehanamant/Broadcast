


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayBusinessesByDate]
	@active bit,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	(
		SELECT	
			business_id,
			code,
			name,
			type,
			active,
			start_date,
			end_date,
			dbo.GetSubscribersForMso(business_id,@effective_date,1,null)
		FROM 
			uvw_business_universe (NOLOCK) 
		WHERE	
			(@active IS NULL OR active=@active)
			AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
		
		UNION

		SELECT	
			id,
			code,
			name,
			type,
			active,
			effective_date,
			null,
			dbo.GetSubscribersForMso(id,@effective_date,1,null)
		FROM 
			businesses (NOLOCK)
		WHERE
			(@active IS NULL OR active=@active)
			AND businesses.id NOT IN (SELECT business_id FROM uvw_business_universe (NOLOCK) WHERE 
				(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
				AND (@active IS NULL OR active=@active)
			)
	) 
	ORDER BY name
END



