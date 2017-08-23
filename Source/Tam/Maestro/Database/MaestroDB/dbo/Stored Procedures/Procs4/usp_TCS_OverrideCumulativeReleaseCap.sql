-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_OverrideCumulativeReleaseCap
	@proposal_id INT,
	@employee_id INT,
	@approval_date DATETIME,
	@approval_amount MONEY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total INT;
	SELECT @total = COUNT(1) FROM dbo.traffic_cap_cumulative_override_approvals tccoa (NOLOCK) WHERE tccoa.proposal_id=@proposal_id;

	IF @total > 0
		-- update approval amount
		UPDATE 
			dbo.traffic_cap_cumulative_override_approvals
		SET
			employee_id=@employee_id,
			approval_date = @approval_date,
			approval_amount = @approval_amount
		WHERE
			proposal_id = @proposal_id;	
	ELSE
		-- insert approval
		INSERT INTO dbo.traffic_cap_cumulative_override_approvals (proposal_id, employee_id, approval_date, approval_amount) 
			VALUES (@proposal_id, @employee_id, @approval_date, @approval_amount);
			
	-- remove from queue
	DELETE FROM traffic_cumulative_cap_queues WHERE proposal_id=@proposal_id;
END
