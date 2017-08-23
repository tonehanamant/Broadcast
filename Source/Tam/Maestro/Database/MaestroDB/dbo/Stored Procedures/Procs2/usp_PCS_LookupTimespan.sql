-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_LookupTimespan
	@start_time INT,
	@end_time INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		start_time,
		end_Time 
	FROM 
		timespans (NOLOCK) 
	WHERE 
		start_time=@start_time 
		AND end_time=@end_time
END
