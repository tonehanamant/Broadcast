-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_QueueCumulativeCapCheck
	@proposal_id INT,
	@employee_id INT
AS
BEGIN
	SET NOCOUNT ON;	
	IF (SELECT COUNT(1) FROM dbo.traffic_cumulative_cap_queues tccq (NOLOCK) WHERE tccq.proposal_id=@proposal_id) = 0
		INSERT INTO dbo.traffic_cumulative_cap_queues(proposal_id, employee_id, date_created) VALUES (@proposal_id, @employee_id, GETDATE());
END
