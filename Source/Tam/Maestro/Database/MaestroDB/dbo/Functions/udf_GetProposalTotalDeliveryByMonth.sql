	-- =============================================
	-- Author:		Stephen DeFusco
	-- Create date: 2/18/2015
	-- Description:	Gets total delivery of a media plan for the specified demo and media month based on the worksheet of the media plan. Not valid for posting plans.
	-- =============================================
	CREATE FUNCTION [dbo].[udf_GetProposalTotalDeliveryByMonth]
	(
		@proposal_id INT,
		@audience_id INT,
		@media_month_id INT
	)
	RETURNS FLOAT
	AS
	BEGIN
		DECLARE @return AS FLOAT
	
		SET @return = (
			SELECT
				SUM(
					CAST(pdw.units AS FLOAT)
					*
					CASE p.is_equivalized
						WHEN 1 THEN
							((pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0) * sl.delivery_multiplier
						ELSE
							(pda.us_universe * pd.universal_scaling_factor * pda.rating) / 1000.0
					END
				)
			FROM
				proposal_detail_audiences pda (NOLOCK)
				JOIN proposal_details pd (NOLOCK) ON pd.id=pda.proposal_detail_id
				JOIN proposal_detail_worksheets pdw (NOLOCK) ON pdw.proposal_detail_id=pd.id
				JOIN media_weeks mw (NOLOCK) ON mw.id=pdw.media_week_id
					AND mw.media_month_id=@media_month_id
				JOIN proposals p (NOLOCK) ON p.id=pd.proposal_id
				JOIN spot_lengths sl (NOLOCK) ON sl.id=pd.spot_length_id
			WHERE
				pda.audience_id=@audience_id
				AND pd.proposal_id=@proposal_id
		)
	
		RETURN @return
	END
