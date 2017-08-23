


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_selectDisplayBusinessByDate]
	@business_id int,
	@effective_date datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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
		(start_date<=@effective_date AND (end_date>=@effective_date OR end_date IS NULL)) 
		AND business_id=@business_id
END



