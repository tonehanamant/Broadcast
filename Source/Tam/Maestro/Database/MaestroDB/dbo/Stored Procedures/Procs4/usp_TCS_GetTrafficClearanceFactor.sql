-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/6/2014
-- Description:	
-- =============================================
-- usp_TCS_GetTrafficClearanceFactor 34420, '5/2/2012'
CREATE PROCEDURE [dbo].[usp_TCS_GetTrafficClearanceFactor]
	@traffic_id INT,
	@effective_date DATETIME
AS
BEGIN
	SELECT dbo.udf_GetTrafficClearanceFactor(@traffic_id,@effective_date);
END
