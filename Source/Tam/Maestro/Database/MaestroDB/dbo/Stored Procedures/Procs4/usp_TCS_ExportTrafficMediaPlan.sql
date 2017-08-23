-- =============================================
-- Author:		Joe Jacobs
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- Changes:		2/3/2011	Removed reference to proposal_details.inactive field (it was deleted).
--				2/3/2017    Removed join to companies table
-- =============================================
CREATE PROCEDURE [dbo].[usp_TCS_ExportTrafficMediaPlan]
	@proposal_id int,
	@traffic_id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SELECT 
		products.name,
		proposals.advertiser_company_id,
		proposals.agency_company_id,
		proposals.name,
		spot_lengths.length,
		proposals.default_daypart_id,
		proposals.flight_text,
		proposals.date_created,
		dbo.GetProposalVersionIdentifier(proposals.id) 'version_identifier', 
		proposals.is_equivalized,
		proposals.rate_card_type_id,
		proposals.proposal_status_id,
		proposals.guarantee_type,
		proposals.start_date,
		proposals.end_date,
		proposals.base_ratings_media_month_id,
		proposals.base_universe_media_month_id,
		proposals.rating_source_id,
		proposals.buyer_note,
		proposals.print_title
	FROM 
		proposals (NOLOCK)
		LEFT JOIN products (NOLOCK) ON products.id=proposals.product_id 
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposals.default_spot_length_id 
	WHERE 
		proposals.id=@proposal_id
	SELECT 
		audience_id,
		universe 
	FROM 
		proposal_audiences (NOLOCK) 
	WHERE 
		proposal_id=@proposal_id 
	ORDER BY 
		ordinal
	SELECT 
		a.name 
	FROM 
		proposal_audiences pa (NOLOCK) 
		JOIN audiences a (NOLOCK) ON a.id=pa.audience_id
	WHERE 
		pa.proposal_id=@proposal_id 
		AND pa.ordinal=1
	SELECT 
		proposal_details.id,
		proposal_details.topography_universe,
		proposal_details.proposal_rate,
		n.code,
		proposal_details.num_spots,
		proposal_details.include,
		proposal_details.daypart_id,
		proposal_details.network_id,
		spot_lengths.id,
		spot_lengths.length,
		spot_lengths.delivery_multiplier,
		spot_lengths.order_by,
		spot_lengths.is_default,
		tdpdm.proposal_rate,
		SUM(tdpdm.proposal_spots)
	FROM
		proposal_details (NOLOCK) 
		JOIN proposals p (NOLOCK) ON p.id=proposal_details.proposal_id
		JOIN uvw_network_universe n	(NOLOCK) ON n.network_id=proposal_details.network_id 
			AND (n.start_date<=p.start_date AND (n.end_date>=p.start_date OR n.end_date IS NULL))
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=proposal_details.spot_length_id 
		JOIN traffic_details_proposal_details_map tdpdm (NOLOCK) on tdpdm.proposal_detail_id = proposal_details.id		
	WHERE 
		proposal_details.proposal_id=@proposal_id
		AND tdpdm.traffic_detail_id in (select id from traffic_details (NOLOCK) where traffic_id = @traffic_id)
	GROUP BY
		proposal_details.id,
		proposal_details.topography_universe,
		proposal_details.proposal_rate,
		n.code,
		proposal_details.num_spots,
		proposal_details.include,
		proposal_details.daypart_id,
		proposal_details.network_id,
		spot_lengths.id,
		spot_lengths.length,
		spot_lengths.delivery_multiplier,
		spot_lengths.order_by,
		spot_lengths.is_default,
		tdpdm.proposal_rate
	ORDER BY 
		n.code
	SELECT proposal_details.id,proposal_detail_audiences.audience_id,proposal_detail_audiences.rating,proposal_detail_audiences.us_universe FROM proposal_detail_audiences (NOLOCK) 
		JOIN proposal_details (NOLOCK) ON proposal_details.id=proposal_detail_audiences.proposal_detail_id
	WHERE 
		proposal_details.proposal_id=@proposal_id 
		AND proposal_detail_audiences.audience_id IN (SELECT audience_id FROM proposal_audiences (NOLOCK) WHERE proposal_id=@proposal_id AND ordinal IN (0,1))
	SELECT 
		start_date,
		end_date 
	FROM 
		proposal_flights (NOLOCK) 
	WHERE 
		proposal_id=@proposal_id 
		AND selected=0
END
