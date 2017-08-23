-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/9/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetPostAggregationSystemFactorsByDate]
	@effective_date DATETIME
AS
BEGIN
	SELECT
		pasf.*
	FROM
		post_aggregation_system_factors pasf WITH(NOLOCK)
	WHERE
		(pasf.start_date<=@effective_date AND (pasf.end_date>=@effective_date OR pasf.end_date IS NULL))
END
