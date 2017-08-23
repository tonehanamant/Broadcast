
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_RejectMonthlyReleaseCap
	@traffic_id INT,
	@employee_id INT,
	@rejection_date DATETIME,
	@rejection_amount MONEY
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @total INT;
	SELECT @total = COUNT(1) FROM dbo.traffic_cap_monthly_override_rejections tcmos (NOLOCK) WHERE tcmos.traffic_id=@traffic_id;

	IF @total > 0
		-- update rejection amount
		UPDATE 
			dbo.traffic_cap_monthly_override_rejections
		SET
			employee_id=@employee_id,
			rejection_date = @rejection_date,
			rejection_amount = @rejection_amount
		WHERE
			traffic_id = @traffic_id;	
	ELSE
		-- insert approval
		INSERT INTO dbo.traffic_cap_monthly_override_rejections (traffic_id, employee_id, rejection_date, rejection_amount) 
			VALUES (@traffic_id, @employee_id, @rejection_date, @rejection_amount);
			
	-- remove from queue
	DELETE FROM traffic_monthly_cap_queues WHERE traffic_id=@traffic_id;
END
