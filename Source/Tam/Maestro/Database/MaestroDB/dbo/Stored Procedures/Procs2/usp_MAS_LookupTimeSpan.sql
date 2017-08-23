-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MAS_LookupTimeSpan
	@start_time INT,
	@end_time INT
AS
BEGIN
	SELECT 
		id,
		start_time,
		end_Time 
	FROM 
		timespans 
	WHERE 
		start_time=@start_time 
		AND end_time=@end_time
END
