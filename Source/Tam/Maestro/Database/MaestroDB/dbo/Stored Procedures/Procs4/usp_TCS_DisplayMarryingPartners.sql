-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
-- usp_TCS_DisplayMarryingPartners '40147,40146,40145'
CREATE PROCEDURE usp_TCS_DisplayMarryingPartners
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	SELECT
		dp.id,
		dp.version_number,
		dp.total_gross_cost,
		dp.advertiser,
		dp.product,
		dp.agency,
		dp.title,
		dp.salesperson,
		dp.flight_text,
		dp.include_on_availability_planner,
		dp.date_created,
		dp.date_last_modified,
		dp.status,
		dp.length,
		rct.name,
		dp.original_proposal_id
	FROM
		dbo.uvw_display_proposals dp
		JOIN dbo.rate_card_types rct (NOLOCK) ON rct.id=dp.rate_card_type_id
	WHERE
		dp.id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
END
