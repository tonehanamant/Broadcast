-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/17/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PLS_DeleteClearanceTrafficEstimatesMediaWeek
	@media_week_id INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @partition_number INT;
	
    SELECT @partition_number = $partition.MediaWeekPFN(media_week_id) FROM dbo.clearance_traffic_estimates (NOLOCK) WHERE media_week_id=@media_week_id;

	IF @partition_number IS NOT NULL
	BEGIN
		ALTER TABLE dbo.clearance_traffic_estimates
			SWITCH PARTITION @partition_number TO dbo.clearance_traffic_estimates_out PARTITION @partition_number;
			
		TRUNCATE TABLE dbo.clearance_traffic_estimates_out;
	END
END
