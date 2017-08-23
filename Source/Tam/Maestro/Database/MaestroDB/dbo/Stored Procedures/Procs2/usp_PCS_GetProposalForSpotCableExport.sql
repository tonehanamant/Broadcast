
-- =============================================
-- Author:			Stephen DeFusco
-- Create date:		10/5/2009
-- Modified date:	4/16/2010
-- Description:		<Description,,>
-- =============================================
-- EXEC usp_PCS_GetProposalForSpotCableExport 26758 
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalForSpotCableExport]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CAST(CASE WHEN proposals.original_proposal_id IS NULL THEN proposals.id ELSE proposals.original_proposal_id END AS VARCHAR(25)) + (CASE WHEN proposals.version_number > 0 THEN '-' + CAST(proposals.version_number AS VARCHAR(10)) ELSE '' END) + ' ' 
		[document.name],
		proposals.date_created [campaign.key.date],
		'NCC' [campaign.key.codeOwner],
		CASE WHEN proposals.original_proposal_id IS NULL THEN proposals.id ELSE proposals.original_proposal_id END [campaign.key.codeOwner.id],
		CASE WHEN proposals.version_number > 0 THEN CAST(proposals.version_number AS VARCHAR(10)) ELSE '' END [campaign.key.codeOwner.version],
		proposals.start_date [campaign.dateRange.startDate],
		proposals.end_date [campaign.dateRange.endDate],
		sales_models.scx_name [campaign.company.rep.name],
		sales_models.scx_office [campaign.company.rep.office.name],
		sales_models.scx_country [campaign.company.rep.office.address.country],
		'mailing' [campaign.company.rep.office.address.type],
		sales_models.scx_street [campaign.company.rep.office.address.street],
		sales_models.scx_city [campaign.company.rep.office.address.city],
		sales_models.scx_state [campaign.company.rep.office.address.state.code],
		sales_models.scx_zip [campaign.company.rep.office.address.postalCode],
		sales_models.scx_country [campaign.company.rep.office.phone.country],
		'voice' [campaign.company.rep.office.phone.type],
		sales_models.scx_contact_phone [campaign.company.rep.office.phone],
		sales_models.scx_country [campaign.company.rep.office.fax.country],
		'fax' [campaign.company.rep.office.fax.type],
		sales_models.scx_contact_fax [campaign.company.rep.office.fax],
		'' [campaign.company.rep.office.id.comment],
		'' [campaign.company.rep.office.id.code.codeOwner],
		'Universal Agency Office ID' [campaign.company.rep.office.id.code.codeDescription],
		sales_models.scx_ncc_universal_agency_office_id [campaign.company.rep.office.id.code],

		'DDS' [campaign.agency.office.id2.comment],
		'DDS' [campaign.agency.office.id2.code.codeOwner],
		'Agency Code' [campaign.agency.office.id2.code.codeDescription],

		sales_models.scx_contact_first_name [campaign.company.rep.contact.firstName],
		sales_models.scx_contact_last_name [campaign.company.rep.contact.lastName],
		sales_models.scx_contact_email [campaign.company.rep.contact.email],
		sales_models.scx_country [campaign.company.rep.contact.phone.country],
		'voice' [campaign.company.rep.contact.phone.type],
		sales_models.scx_contact_phone [campaign.company.rep.contact.phone],
		sales_models.scx_country [campaign.company.rep.contact.fax.country],
		'fax' [campaign.company.rep.contact.fax.type],
		sales_models.scx_contact_fax [campaign.company.rep.contact.fax],
		'' [campaign.company.rep.contact.id.comment],
		'' [campaign.company.rep.contact.id.code.codeOwner],
		'' [campaign.company.rep.contact.id.code.codeDescription],
		'' [campaign.company.rep.id.comment],
		'SpotCable' [campaign.company.rep.id.code.codeOwner],
		'' [campaign.company.rep.id.code.codeDescription],
		'NCC' [campaign.company.rep.id.code],
		'' [campaign.agency.name],
		'' [campaign.agency.office.name],
		sales_models.scx_country [campaign.agency.office.address.country],
		'mailing' [campaign.agency.office.address.type],
		'' [campaign.agency.office.address.street],
		'' [campaign.agency.office.address.city],
		'' [campaign.agency.office.address.state.code],
		'' [campaign.agency.office.address.postalCode],
		sales_models.scx_country [campaign.agency.office.phone.country],
		'voice' [campaign.agency.office.phone.type],
		sales_models.scx_contact_phone [campaign.agency.office.phone],
		sales_models.scx_country [campaign.agency.office.fax.country],
		'fax' [campaign.agency.office.fax.type],
		sales_models.scx_contact_fax [campaign.agency.office.fax],
		'' [campaign.agency.office.id.comment],
		'NCC' [campaign.agency.office.id.code.codeOwner],
		'Universal Agency Office ID' [campaign.agency.office.id.code.codeDescription],
		sales_models.scx_ncc_universal_agency_office_id [campaign.agency.office.id.code],
		'' [campaign.agency.id.comment],
		sales_models.scx_name [campaign.agency.id.code.codeOwner],
		'Universal Agency Office ID' [campaign.agency.id.code.codeDescription],
		sales_models.scx_ncc_universal_agency_office_id [campaign.agency.id.code],
		sales_models.scx_name [campaign.advertiser.name],
		'companies.id' [campaign.advertiser.code.codeDescription],
		proposals.advertiser_company_id [campaign.advertiser.code],

		'DDS' [campaign.advertiser.id2.code.codeOwner],
		'Agency Advertiser Code' [campaign.agency.id2.code.codeDescription],

		proposals.name [campaign.prodcut.name],
		products.name [campaign.prodcut.comment],
		'proposals.id' [campaign.prodcut.code.codeDescription],
		proposals.id [campaign.prodcut.code],
		proposals.name [campaign.estimate.desc],
		CASE proposals.is_equivalized WHEN 1 THEN 'Equivalized' ELSE 'Unequivalized' END [campaign.estimate.id.comment],
		'Agency' [campaign.estimate.id.code.codeOwner],
		'' [campaign.estimate.id.code.codeDescription],
		'2498' [campaign.estimate.id.code],
		'NCC' [campaign.makeGoodPolicy.code.codeOwner],
		'Makegood within flight' [campaign.makeGoodPolicy.code.codeDescription],
		'S' [campaign.makeGoodPolicy.code],
		'weekly' [campaign.buyType],
		'proposals.id' [campaign.order.key.codeDescription],
		proposals.id [campaign.order.key.id],
		CASE WHEN proposals.version_number > 0 THEN CAST(proposals.version_number AS VARCHAR(10)) ELSE '' END [campaign.order.key.version],
		'ordered' [campaign.order.key.status],
		'new' [campaign.order.key.subStatus],
		proposals.date_created [campaign.order.key.updateDate],
		dbo.GetProposalTotalCost(proposals.id) [campaign.order.totals.cost],
		dbo.GetProposalTotalUnits(proposals.id) [campaign.order.totals.spots],
		'999' [campaign.order.market.nsi_id],
		'National' [campaign.order.market.name],
		'' [campaign.order.comment],
		'System order Key Description' [campaign.order.systemOrder.key.codeDescription],
		proposals.id [campaign.order.systemOrder.key.id],
		CASE WHEN proposals.version_number > 0 THEN CAST(proposals.version_number AS VARCHAR(10)) ELSE '' END [campaign.order.systemOrder.key.version],
		'ordered' [campaign.order.systemOrder.key.status],
		'new' [campaign.order.systemOrder.key.subStatus],
		proposals.date_created [campaign.order.systemOrder.key.updateDate],
		'National Network' [campaign.order.systemOrder.system.name],
		'9999' [campaign.order.systemOrder.system.syscode],
		'0' [campaign.order.systemOrder.affiliateSplit],
		dbo.GetProposalTotalCost(proposals.id) [campaign.order.systemOrder.totals.cost],
		dbo.GetProposalTotalUnits(proposals.id) [campaign.order.systemOrder.totals.spots]
	FROM
		proposals						(NOLOCK)
		LEFT JOIN products				(NOLOCK) ON products.id=proposals.product_id
		LEFT JOIN proposal_audiences	(NOLOCK) ON proposal_audiences.proposal_id=proposals.id AND proposal_audiences.ordinal=1
		JOIN proposal_sales_models		(NOLOCK) ON proposal_sales_models.proposal_id=proposals.id
		JOIN sales_models				(NOLOCK) ON sales_models.id=proposal_sales_models.sales_model_id
	WHERE
		proposals.id=@proposal_id


	SELECT
		CAST(mm.year AS VARCHAR(4)) + '-' + (CASE WHEN mm.month < 10 THEN '0' + CAST(mm.month AS VARCHAR(2)) ELSE CAST(mm.month AS VARCHAR(2)) END) [breakOutMonth.yearMonth],
		SUM(pdw.units * pd.proposal_rate) [breakOutMonth.cost],
		SUM(pdw.units) [breakOutMonth.spots]
	FROM
		proposal_detail_worksheets pdw	(NOLOCK)
		JOIN proposal_details pd		(NOLOCK) ON pd.id=pdw.proposal_detail_id
		JOIN media_weeks mw				(NOLOCK) ON mw.id=pdw.media_week_id
		JOIN media_months mm			(NOLOCK) ON mm.id=mw.media_month_id
	WHERE
		pd.proposal_id=@proposal_id
		AND pdw.units > 0
	GROUP BY
		mm.year,
		mm.month
	ORDER BY
		mm.year,
		mm.month		


	SELECT
		proposal_flights.start_date [weeks.startDate],
		media_weeks.id [media_week_id]
	FROM
		proposal_flights
		JOIN media_weeks (NOLOCK) ON (media_weeks.start_date <= proposal_flights.end_date AND media_weeks.end_date >= proposal_flights.start_date)
	WHERE
		proposal_id=@proposal_id
		AND selected=1
	ORDER BY
		proposal_flights.start_date


	SELECT
		proposal_details.id,
		dbo.GetLongReadableTime(dp.start_time) [startTime],
		dbo.GetLongReadableTime(dp.end_time) [endTime],
		'M' [startDay],
		CASE dp.mon WHEN 1 THEN 'Y' ELSE 'N' END [Monday],
		CASE dp.tue WHEN 1 THEN 'Y' ELSE 'N' END [Tuesday],
		CASE dp.wed WHEN 1 THEN 'Y' ELSE 'N' END [Wednesday],
		CASE dp.thu WHEN 1 THEN 'Y' ELSE 'N' END [Thursday],
		CASE dp.fri WHEN 1 THEN 'Y' ELSE 'N' END [Friday],
		CASE dp.sat WHEN 1 THEN 'Y' ELSE 'N' END [Saturday],
		CASE dp.sun WHEN 1 THEN 'Y' ELSE 'N' END [Sunday],
		'PT' + CAST(spot_lengths.length AS VARCHAR(10)) + 'S' [length],
		dp.code [daypartCode],
		'Various' [program],
		'' [comment],
		proposal_details.network_id,
		CASE WHEN nielsen_networks.name IS NULL THEN daypart_nielsen_networks.name ELSE nielsen_networks.name END [network.name],
		CASE WHEN nielsen_networks.code IS NULL THEN daypart_nielsen_networks.code ELSE nielsen_networks.code END [network.code],
		'NCC' [network.codeOwner],
		CASE WHEN nielsen_networks.nielsen_id IS NULL THEN daypart_nielsen_networks.nielsen_id ELSE nielsen_networks.nielsen_id END [network.codeDescription],
		proposal_details.proposal_rate [spotCost],
		dbo.GetProposalDetailSubTotalCost(proposal_details.id) [totals.cost],
		proposal_details.num_spots [totals.spots]
	FROM
		proposal_details									(NOLOCK)
		JOIN vw_ccc_daypart dp								(NOLOCK) ON dp.id=proposal_details.daypart_id
		JOIN spot_lengths									(NOLOCK) ON spot_lengths.id=proposal_details.spot_length_id
		LEFT JOIN network_maps daypart_network_maps			(NOLOCK) ON daypart_network_maps.network_id=proposal_details.network_id AND daypart_network_maps.map_set='DaypartNetworks'
		LEFT JOIN network_maps nielsen_network_maps			(NOLOCK) ON nielsen_network_maps.network_id=proposal_details.network_id AND nielsen_network_maps.map_set='Nielsen'
		LEFT JOIN network_maps nielsen_daypart_network_maps (NOLOCK) ON nielsen_daypart_network_maps.network_id=CAST(daypart_network_maps.map_value AS INT) AND nielsen_daypart_network_maps.map_set='Nielsen'
		LEFT JOIN nielsen_networks							(NOLOCK) ON nielsen_networks.nielsen_id=CAST(nielsen_network_maps.map_value AS INT)
		LEFT JOIN nielsen_networks daypart_nielsen_networks (NOLOCK) ON daypart_nielsen_networks.nielsen_id=CAST(nielsen_daypart_network_maps.map_value AS INT)
	WHERE
		proposal_details.proposal_id=@proposal_id
		AND proposal_details.num_spots>0


	SELECT
		pdw.proposal_detail_id,
		mw.id [media_week_id],
		pdw.units
	FROM
		proposal_detail_worksheets pdw	(NOLOCK)
		JOIN proposal_details pd		(NOLOCK) ON pd.id=pdw.proposal_detail_id
		JOIN media_weeks mw				(NOLOCK) ON mw.id=pdw.media_week_id
	WHERE
		pd.proposal_id=@proposal_id
		AND pdw.units > 0
	ORDER BY
		mw.start_date

	SELECT
		pa.*,
		a.*
	FROM
		proposal_audiences pa	(NOLOCK)
		JOIN audiences a		(NOLOCK) ON a.id=pa.audience_id
			AND a.category_code=0 --sex
	WHERE
		pa.proposal_id=@proposal_id
	ORDER BY
		pa.ordinal

	
	SELECT
		pda.proposal_detail_id,
		pda.audience_id,
		pda.rating * 100.0 [rating],
		dbo.GetProposalDetailDelivery(pda.proposal_detail_id, pda.audience_id) [impressions]
	FROM
		proposal_detail_audiences pda	(NOLOCK)
		JOIN proposal_details pd		(NOLOCK) ON pda.proposal_detail_id=pd.id
		JOIN audiences a				(NOLOCK) ON a.id=pda.audience_id
			AND a.category_code=0 --sex
	WHERE
		pd.proposal_id=@proposal_id
END
