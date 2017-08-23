
CREATE Procedure [dbo].[usp_REL_GetAllDisplayTraffic]
(
	@start_date Datetime,
	@end_date Datetime
)
AS
BEGIN
declare @query as varchar(max);

set @query = 'with strippedtraffic (id, revision)
as (
	select case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end [ID],
	max(traffic.revision)
			from traffic (NOLOCK) 
		where traffic.status_id in (24)
	group by case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end
),
alltraffic (id, original_traffic_id, revision, trafficname, companiesid, productsname, startdate, enddate, status, spotlength, spotlengthid)
as
(
select traffic.id, case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end,
 traffic.revision, traffic.name, proposals.advertiser_company_id, products.name, traffic.start_date, traffic.end_date, statuses.name,
	(SELECT  TOP 1 WITH ties spot_lengths.length
						 FROM  traffic_details td WITH (NOLOCK)
						 JOIN spot_lengths WITH (NOLOCK) on spot_lengths.id = td.spot_length_id
						 WHERE traffic.id = td.traffic_id
						 GROUP  BY spot_lengths.length
						 ORDER  BY COUNT(*) DESC),
	(SELECT  TOP 1 WITH ties spot_lengths.id
						 FROM  traffic_details td WITH (NOLOCK)
						 JOIN spot_lengths WITH (NOLOCK) on spot_lengths.id = td.spot_length_id
						 WHERE traffic.id = td.traffic_id
						 GROUP  BY spot_lengths.id
						 ORDER  BY COUNT(*) DESC)
			from traffic (NOLOCK) join traffic_proposals (NOLOCK) on traffic.id = traffic_proposals.traffic_id and traffic_proposals.primary_proposal = 1
			join proposals (NOLOCK) on proposals.id = traffic_proposals.proposal_id
			join statuses (NOLOCK) on statuses.id = traffic.status_id
			join strippedtraffic on strippedtraffic.id = case when traffic.original_traffic_id IS NULL then traffic.id else traffic.original_traffic_id end
				 and strippedtraffic.revision = traffic.revision
			left join products (NOLOCK) on proposals.product_id = products.id
			where traffic.status_id in (24)
)
select id, original_traffic_id, revision, trafficname, companiesid, productsname, startdate, enddate, status,
spotlength, spotlengthid
	 from alltraffic where 1=1 ';

if(@start_date is not null)
begin
	set @query = @query + ' and startdate >= ''' + convert(varchar, @start_date, 101) + ''''; 
end;

if(@end_date is not null)
begin
	set @query =  @query + ' and enddate <= ''' + convert(varchar, @end_date, 101) + ''''; 
end;

set @query = @query + ' order by original_traffic_id DESC';

print @query;


exec (@query);

END

