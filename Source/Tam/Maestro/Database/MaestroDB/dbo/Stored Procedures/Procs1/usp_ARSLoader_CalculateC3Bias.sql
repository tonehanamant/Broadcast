
-- =============================================
-- Author:		Mike Deaven
-- Create date: 8/30/2012
-- Description:	Calculates C3 Bias
-- =============================================
CREATE PROCEDURE usp_ARSLoader_CalculateC3Bias 
	@monthStart varchar(5)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	insert into
		c3_biases(
			media_month_id,
			daypart_id, 
			nielsen_network_id, 
			audience_id, 
			bias
		)
		SELECT
			mm_dst.id media_month_id,
			c3.daypart_id, 
			c3.nielsen_network_id, 
			c3.audience_id, 
			c3.bias
		FROM
			media_months mm_src
			join media_months mm_dst on
				mm_src.month = mm_dst.month
				and
				mm_src.year = mm_dst.year - 1
			join c3_biases c3 on
			mm_src.id = c3.media_month_id
		where
			mm_src.media_month = @monthStart;
END
