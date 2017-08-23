
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_RejectCumulativeReleaseCap
	@proposal_id INT,
	@employee_id INT,
	@rejection_date DATETIME,
	@rejection_amount MONEY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total INT;
	SELECT @total = COUNT(1) FROM dbo.traffic_cap_cumulative_override_rejections tccos (NOLOCK) WHERE tccos.proposal_id=@proposal_id;

	IF @total > 0
		-- update approval amount
		UPDATE 
			dbo.traffic_cap_cumulative_override_rejections
		SET
			employee_id=@employee_id,
			rejection_date = @rejection_date,
			rejection_amount = @rejection_amount
		WHERE
			proposal_id = @proposal_id;	
	ELSE
		-- insert approval
		INSERT INTO dbo.traffic_cap_cumulative_override_rejections (proposal_id, employee_id, rejection_date, rejection_amount) 
			VALUES (@proposal_id, @employee_id, @rejection_date, @rejection_amount);
			
	-- remove from queue
	DELETE FROM traffic_cumulative_cap_queues WHERE proposal_id=@proposal_id;
END
