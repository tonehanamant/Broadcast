
-- =============================================
-- Author:		Joe Jacobs
-- Modified:	Stephen DeFusco 12/2/2013 - Changed from dynamic SQL to straight SQL and reformatted.
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_REL_GetDisplayTrafficNotReleased NULL, NULL
CREATE Procedure [dbo].[usp_REL_GetDisplayTrafficNotReleased]
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	with 
	strippedtraffic (id, revision) as (
		select 
			case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end [ID],
			max(traffic.revision)
		from 
			traffic (NOLOCK) 
		where 
			traffic.status_id in (24)
		group by 
			case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end
	),
	alltraffic (id, original_traffic_id, revision, trafficname, companiesid, productsname, startdate, enddate, status, spotlength, spotlengthid) as (
		select 
			traffic.id, 
			case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end,
			traffic.revision, 
			traffic.name, 
			proposals.advertiser_company_id, 
			products.name, 
			traffic.start_date, 
			traffic.end_date, 
			statuses.name,
			(
				SELECT  TOP 1 WITH ties spot_lengths.length FROM  traffic_details td WITH (NOLOCK) JOIN spot_lengths WITH (NOLOCK) on spot_lengths.id = td.spot_length_id WHERE traffic.id = td.traffic_id GROUP  BY spot_lengths.length ORDER  BY COUNT(*) DESC
			),
			(
				SELECT  TOP 1 WITH ties td.spot_length_id FROM  traffic_details td WITH (NOLOCK) WHERE traffic.id = td.traffic_id GROUP  BY td.spot_length_id ORDER  BY COUNT(*) DESC
			)
		from 
			traffic WITH (NOLOCK) 
			join traffic_proposals WITH (NOLOCK) on traffic.id = traffic_proposals.traffic_id 
				and traffic_proposals.primary_proposal = 1
			join proposals WITH (NOLOCK) on proposals.id = traffic_proposals.proposal_id
			join statuses WITH (NOLOCK) on statuses.id = traffic.status_id
			join strippedtraffic on strippedtraffic.id = case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end
				and strippedtraffic.revision = traffic.revision
			left join products WITH (NOLOCK) on proposals.product_id = products.id
		where 
			traffic.release_id is null 
			and traffic.status_id in (24)
	)
		
	select 
		id, 
		original_traffic_id, 
		revision, 
		trafficname, 
		companiesid, 
		productsname, 
		startdate, 
		enddate, 
		status, 
		spotlength, 
		spotlengthid
	from 
		alltraffic 
	where 
		spotlengthid is not null
		and (@start_date IS NULL OR startdate >= @start_date)
		and (@end_date IS NULL OR @end_date <= @end_date)
	order by 
		original_traffic_id DESC
END