	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 2/18/2015
	-- Description:	<Description, ,>
	-- =============================================
	CREATE FUNCTION [dbo].[udf_GetProposalCashBackImpressions]
	(
		@ordered_proposal_id INT,
		@media_month_id INT,
		@audience_id INT
	)
	RETURNS FLOAT
	AS
	BEGIN
		DECLARE @cash_back_percentage FLOAT;
		SET @cash_back_percentage = dbo.udf_GetProposalCashBackPercentage(@ordered_proposal_id, @media_month_id);
	
		DECLARE @contracted_monthly_impressions FLOAT;
		SET @contracted_monthly_impressions = dbo.udf_GetProposalTotalDeliveryByMonth(@ordered_proposal_id, @audience_id, @media_month_id);
			
		RETURN ISNULL(@contracted_monthly_impressions * (1 - @cash_back_percentage), 0);
	END
