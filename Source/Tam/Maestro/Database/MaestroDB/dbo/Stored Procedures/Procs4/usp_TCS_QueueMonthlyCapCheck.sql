-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_QueueMonthlyCapCheck
	@traffic_id INT,
	@employee_id INT
AS
BEGIN
	SET NOCOUNT ON;	
	IF (SELECT COUNT(1) FROM dbo.traffic_monthly_cap_queues tmcq (NOLOCK) WHERE tmcq.traffic_id=@traffic_id) = 0
		INSERT INTO dbo.traffic_monthly_cap_queues(traffic_id, employee_id, date_created) VALUES (@traffic_id, @employee_id, GETDATE());
END
