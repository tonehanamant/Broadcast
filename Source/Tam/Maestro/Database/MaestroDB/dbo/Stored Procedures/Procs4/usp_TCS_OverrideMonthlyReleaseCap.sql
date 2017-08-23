-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_OverrideMonthlyReleaseCap
	@traffic_id INT,
	@employee_id INT,
	@approval_date DATETIME,
	@approval_amount MONEY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total INT;
	SELECT @total = COUNT(1) FROM dbo.traffic_cap_monthly_override_approvals tcmoa (NOLOCK) WHERE tcmoa.traffic_id=@traffic_id;

	IF @total > 0
		-- update approval amount
		UPDATE 
			dbo.traffic_cap_monthly_override_approvals
		SET
			employee_id=@employee_id,
			approval_date = @approval_date,
			approval_amount = @approval_amount
		WHERE
			traffic_id = @traffic_id;	
	ELSE
		-- insert approval
		INSERT INTO dbo.traffic_cap_monthly_override_approvals (traffic_id, employee_id, approval_date, approval_amount) 
			VALUES (@traffic_id, @employee_id, @approval_date, @approval_amount);
			
	-- remove from queue
	DELETE FROM traffic_monthly_cap_queues WHERE traffic_id=@traffic_id;
END
