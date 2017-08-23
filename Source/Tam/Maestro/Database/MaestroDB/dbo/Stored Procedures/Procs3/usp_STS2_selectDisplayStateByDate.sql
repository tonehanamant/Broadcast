



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayStateByDate]
	@state_id INT,
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT	
		state_id,
		code,
		name,
		active,
		flag,
		start_date,
		end_date,
		dbo.GetSubscribersForState(state_id,@effective_date,1,null)
	FROM 
		uvw_state_universe (NOLOCK)
	WHERE 
		state_id=@state_id
		AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 

	UNION

	SELECT	
		id,
		code,
		name,
		active,
		flag,
		effective_date,
		null,
		dbo.GetSubscribersForState(id,@effective_date,1,null)
	FROM 
		states (NOLOCK)
	WHERE 
		id=@state_id
		AND id NOT IN (
			SELECT state_id FROM uvw_state_universe (NOLOCK) WHERE
				state_id=@state_id
				AND (start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL))
		)
END




