-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetDaysForDaypart
	@daypart_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		code_1,
		code_2,
		code_3,
		name,
		ordinal 
	FROM 
		days (NOLOCK)
	WHERE 
		id IN (
			SELECT day_id FROM daypart_days (NOLOCK) WHERE daypart_id=@daypart_id
		)
END
