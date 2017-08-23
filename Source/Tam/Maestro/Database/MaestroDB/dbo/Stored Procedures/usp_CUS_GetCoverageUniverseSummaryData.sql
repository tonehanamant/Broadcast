


/***************************************************************************************************
** Date			Author			Description	
** ---------	------------	-------------------------------------------------------------------
** 12/08/2015	Abdul Sukkur	Get coverage universe summary data
** 01/06/2016	Abdul Sukkur	changed regional network logic to take only one zone network if more than one zone network found for the same zone
** 05/16/2017	Abdul Sukkur	MME-1374-Custom Traffic SBTs will not affect MVPD's coverage universe
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_CUS_GetCoverageUniverseSummaryData]
	@media_month_id int
AS
BEGIN

CREATE TABLE #coverage_universe_summary 
(
 media_month_id int, 
 network_id int, 
 network_code varchar(15), 
 owned_business_id int, 
 managed_business_id int, 
 dma_id int, zone_id int, 
 subscribers float
)



	DECLARE @effective_date AS DATETIME;
	SELECT @effective_date = [start_date] 
		FROM media_months (NOLOCK) WHERE id =@media_month_id

	CREATE TABLE #network_maps  (network_id INT, regional_network_id INT)
					INSERT INTO #network_maps
									SELECT network_id, CAST(map_value AS INT) 
													FROM dbo.udf_GetNetworkMapsAsOf(@effective_date) 
									WHERE map_set='PostReplace'
									UNION
									SELECT network_id, CAST(map_value AS INT) 
													FROM dbo.udf_GetNetworkMapsAsOf(@effective_date) 
									WHERE map_set='DaypartNetworks'
                                
	-- Get trafficable zone groups
	CREATE TABLE #traffic_zone_groups (primary_zone_id int, secondary_zone_id int);
	INSERT INTO #traffic_zone_groups (primary_zone_id,secondary_zone_id) 
	select distinct ozg.primary_zone_id, ozg.secondary_zone_id 
	from 
					uvw_zonezone_universe ozg 
					join uvw_zone_universe zg on ozg.primary_zone_id = zg.zone_id and 
									(zg.start_date<=@effective_date AND (zg.end_date>=@effective_date OR zg.end_date IS NULL)) 
					join uvw_systemzone_universe szu on szu.zone_id = zg.zone_id and szu.type = 'TRAFFIC' AND
									(szu.start_date<=@effective_date AND (szu.end_date>=@effective_date OR szu.end_date IS NULL)) 
					JOIN uvw_system_universe s (NOLOCK) ON s.system_id = szu.system_id and s.active = 1 and s.custom_traffic_system = 0
			and (s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)) 
	   join uvw_zone_universe lz on ozg.secondary_zone_id = lz.zone_id and 
									(lz.start_date<=@effective_date AND (lz.end_date>=@effective_date OR lz.end_date IS NULL)) 
					where
									zg.active = 1 
									and zg.type = 'Zone Group'
									and zg.traffic = 1 
									and lz.active = 1 
									and lz.[primary] = 1
									and (ozg.start_date<=@effective_date AND (ozg.end_date>=@effective_date OR ozg.end_date IS NULL)); 

	-- create table for storing subscribers
	CREATE TABLE #z_subs (business_id int, zone_id int, network_id int, dma_id int, subscribers float);
	CREATE TABLE #zg_subs (zone_group_id int, zone_id int, network_id int, subscribers float);

	--Get Local Primary Subscribers First
	-- Get all local zones that are not a zone group themselves
	INSERT INTO #z_subs(business_id, zone_id, network_id, dma_id, subscribers)
	select 
					b.business_id [business_id], fz.zone_id [zone_id], CASE WHEN rs.network_id is null then n.network_id else rs.network_id end [network_id], d.dma_id [dma_id], MAX(fzn.subscribers) [subscribers]
	from 
					media_months mm (NOLOCK) join 
					uvw_zonebusiness_universe fzb (NOLOCK) on 
									(fzb.start_date<=mm.start_date AND (fzb.end_date>=mm.start_date OR fzb.end_date IS NULL))
					join uvw_business_universe b (NOLOCK) on b.business_id= fzb.business_id and 
									(b.start_date<=mm.start_date AND (b.end_date>=mm.start_date OR b.end_date IS NULL))
					join uvw_zone_universe fz on fz.zone_id = fzb.zone_id and fzb.[type] = 'MANAGEDBY' 
									and (fz.start_date<=mm.start_date AND (fz.end_date>=mm.start_date OR fz.end_date IS NULL))
					join uvw_zonenetwork_universe fzn (NOLOCK) on fzn.zone_id = fz.zone_id 
									and (fzn.start_date<=mm.start_date AND (fzn.end_date>=mm.start_date OR fzn.end_date IS NULL))
									and fz.zone_id in
						(SELECT fsz.zone_id FROM uvw_systemzone_universe fsz (NOLOCK)   
						 JOIN uvw_system_universe fs (NOLOCK) ON fs.system_id = fsz.system_id 
						 and (fs.start_date<=mm.start_date AND (fs.end_date>=mm.start_date OR fs.end_date IS NULL))
						WHERE (fsz.start_date<=mm.start_date AND (fsz.end_date>=mm.start_date OR fsz.end_date IS NULL)) 
						and fs.active = 1 and fs.custom_traffic_system = 0 and fsz.[type] = 'TRAFFIC')
					join uvw_zonedma_universe fzd (NOLOCK) on fzd.zone_id = fz.zone_id
						and (fzd.start_date<=mm.start_date AND (fzd.end_date>=mm.start_date OR fzd.end_date IS NULL))
					join uvw_dma_universe d (NOLOCK) on d.dma_id = fzd.dma_id and 
									(d.start_date<=mm.start_date AND (d.end_date>=mm.start_date OR d.end_date IS NULL))
					join uvw_network_universe n (NOLOCK) on n.network_id = fzn.network_id and 
									(n.start_date<=mm.start_date AND (n.end_date>=mm.start_date OR n.end_date IS NULL))
					left join #network_maps rs on rs.regional_network_id = fzn.network_id
	where
					mm.id = @media_month_id
					and fz.active = 1 
					and fz.[type] not in('Zone Group')
					and fz.[primary] = 1
					and fz.traffic = 1 
					and fzn.[primary] = 1
					and fzn.trafficable = 1
	GROUP BY
					b.business_id , fz.zone_id , CASE WHEN rs.network_id is null then n.network_id else rs.network_id end , d.dma_id  
                
	-- ZONE GROUP SECTION

	-- FIRST, GET ALL SUBS FOR A ZONE GROUP THAT HAS NOT BEEN COUNTED IN STEP 1 AND IS NOT IN ANOTHER TRAFFICABLE ZONE GROUP
	INSERT INTO #zg_subs (zone_group_id, zone_id, network_id, subscribers)
	select 
					zg.zone_id [zone_group_id],
					fz.zone_id [zone_id], 
					CASE WHEN rs.network_id is null then n.network_id else rs.network_id end [network_id], 
					MAX(fzn.subscribers) [subscribers]
	from 
					media_months mm (NOLOCK)  
					join uvw_business_universe b (NOLOCK) on 
									(b.start_date<=mm.start_date AND (b.end_date>=mm.start_date OR b.end_date IS NULL))
					join uvw_zonebusiness_universe fzb (NOLOCK) on b.business_id= fzb.business_id and 
									(fzb.start_date<=mm.start_date AND (fzb.end_date>=mm.start_date OR fzb.end_date IS NULL))
					join #traffic_zone_groups tzg on fzb.zone_id = tzg.primary_zone_id --already has a traffic system attached
					join uvw_zone_universe zg (NOLOCK) on zg.zone_id = fzb.zone_id and fzb.[type] = 'MANAGEDBY'
									and (zg.start_date<=mm.start_date AND (zg.end_date>=mm.start_date OR zg.end_date IS NULL))
					join uvw_zone_universe fz on fz.zone_id = tzg.secondary_zone_id  --local zone join here
									and (fz.start_date<=mm.start_date AND (fz.end_date>=mm.start_date OR fz.end_date IS NULL))
									and fz.zone_id not in (select distinct zone_id from #z_subs) --makes sure it wasn't already counted as a straight local
									and fz.zone_id not in (select distinct secondary_zone_id from #traffic_zone_groups where primary_zone_id <> zg.zone_id) --make sure its not in another trafficable zone group
									join uvw_zonenetwork_universe fzn (NOLOCK) on fzn.zone_id = fz.zone_id --local networks
													and (fzn.start_date<=mm.start_date AND (fzn.end_date>=mm.start_date OR fzn.end_date IS NULL))  
									join uvw_zonedma_universe fzd (NOLOCK) on fzd.zone_id = zg.zone_id
										and (fzd.start_date<=mm.start_date AND (fzd.end_date>=mm.start_date OR fzd.end_date IS NULL))
									join uvw_dma_universe d (NOLOCK) on d.dma_id = fzd.dma_id and 
													(d.start_date<=mm.start_date AND (d.end_date>=mm.start_date OR d.end_date IS NULL))
									join uvw_network_universe n (NOLOCK) on n.network_id = fzn.network_id and 
													(n.start_date<=mm.start_date AND (n.end_date>=mm.start_date OR n.end_date IS NULL))
									left join #network_maps rs on rs.regional_network_id = fzn.network_id
	where
					mm.id = @media_month_id
					and zg.[type] in('Zone Group')
					and fz.active = 1 
					and zg.active = 1 
					and fz.[primary] = 1
					and zg.traffic = 1 
					and fzn.[primary] = 1
					and fzn.trafficable = 1
	GROUP BY
					zg.zone_id, fz.zone_id, CASE WHEN rs.network_id is null then n.network_id else rs.network_id end


	-- GET SUBS FOR ALL LOCALS IN A ZONE GROUP THAT BELONG TO MULTIPLE ACTIVE ZONE GROUPS
	INSERT INTO #zg_subs (zone_group_id, zone_id, network_id, subscribers)
	select 
					zg.zone_id [zone_group_id],
					fz.zone_id [zone_id], 
					CASE WHEN rs.network_id is null then n.network_id else rs.network_id end [network_id], 
					MAX(fzn.subscribers) [subscribers]
	from 
					media_months mm (NOLOCK)  
					join uvw_business_universe b (NOLOCK) on 
									(b.start_date<=mm.start_date AND (b.end_date>=mm.start_date OR b.end_date IS NULL))
					join uvw_zonebusiness_universe fzb (NOLOCK) on b.business_id= fzb.business_id and 
									(fzb.start_date<=mm.start_date AND (fzb.end_date>=mm.start_date OR fzb.end_date IS NULL))
					join #traffic_zone_groups tzg on fzb.zone_id = tzg.primary_zone_id --already has a traffic system attached
					join uvw_zone_universe zg (NOLOCK) on zg.zone_id = fzb.zone_id and fzb.[type] = 'MANAGEDBY'
									and (zg.start_date<=mm.start_date AND (zg.end_date>=mm.start_date OR zg.end_date IS NULL))
					join uvw_zone_universe fz on fz.zone_id = tzg.secondary_zone_id  --local zone join here
									and (fz.start_date<=mm.start_date AND (fz.end_date>=mm.start_date OR fz.end_date IS NULL))
									and fz.zone_id not in (select distinct zone_id from #z_subs) --makes sure it wasn't already counted as a straight local
									and fz.zone_id in (select distinct secondary_zone_id from #traffic_zone_groups where primary_zone_id <> zg.zone_id) --make sure its not in another trafficable zone group
					-- because it is in multiple zone groups, check to match the managed by flags
					join uvw_zonebusiness_universe fzbl (NOLOCK) on fzbl.zone_id = fz.zone_id and fzbl.type = 'MANAGEDBY' and fzbl.business_id = fzb.business_id and 
									(fzbl.start_date<=mm.start_date AND (fzbl.end_date>=mm.start_date OR fzbl.end_date IS NULL))      
					join uvw_zonenetwork_universe fzn (NOLOCK) on fzn.zone_id = fz.zone_id --local networks
									and (fzn.start_date<=mm.start_date AND (fzn.end_date>=mm.start_date OR fzn.end_date IS NULL))  
					join uvw_zonedma_universe fzd (NOLOCK) on fzd.zone_id = zg.zone_id
									and (fzd.start_date<=mm.start_date AND (fzd.end_date>=mm.start_date OR fzd.end_date IS NULL))
					join uvw_dma_universe d (NOLOCK) on d.dma_id = fzd.dma_id and 
									(d.start_date<=mm.start_date AND (d.end_date>=mm.start_date OR d.end_date IS NULL))
					join uvw_network_universe n (NOLOCK) on n.network_id = fzn.network_id and 
									(n.start_date<=mm.start_date AND (n.end_date>=mm.start_date OR n.end_date IS NULL))
					left join #network_maps rs on rs.regional_network_id = fzn.network_id
	where
					mm.id = @media_month_id
					and zg.[type] in('Zone Group')
					and fz.active = 1 
					and zg.active = 1 
					and fz.[primary] = 1
					and zg.traffic = 1 
					and fzn.[primary] = 1
					and fzn.trafficable = 1
	GROUP BY
					zg.zone_id, fz.zone_id, CASE WHEN rs.network_id is null then n.network_id else rs.network_id end


	-- Finally insert the zone group subs
	INSERT INTO #coverage_universe_summary(media_month_id, network_id, network_code, owned_business_id, managed_business_id, dma_id, zone_id, subscribers)
	SELECT @media_month_id, n.network_id, n.code, fzbOwn.business_id as owned_business_id,  z.business_id as managed_business_id,  z.dma_id, z.zone_id, MAX(z.subscribers) 
	from #z_subs as z
		join uvw_zonebusiness_universe fzbOwn (NOLOCK) on z.zone_id= fzbOwn.zone_id and fzbOwn.type = 'OWNEDBY' and 
			(fzbOwn.[start_date]<=@effective_date AND (fzbOwn.end_date>=@effective_date OR fzbOwn.end_date IS NULL))
		join uvw_network_universe n (NOLOCK)  on z.network_id = n.network_id and n.active = 1 and
			(n.[start_date]<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	GROUP BY 
		n.network_id, n.code, fzbOwn.business_id,  z.business_id, z.dma_id, z.zone_id

	INSERT INTO #coverage_universe_summary(media_month_id, network_id, network_code , owned_business_id, managed_business_id, dma_id, zone_id, subscribers)
	SELECT @media_month_id, n.network_id, n.code, fzbOwn.business_id as owned_business_id, 
		fzbMng.business_id as managed_business_id, fzd.dma_id, z.zone_group_id as zone_id, MAX(z.subscribers) as subscribers
	from (select zone_group_id, network_id, sum(subscribers) as subscribers from #zg_subs zg GROUP BY zg.zone_group_id, zg.network_id) as z 
		join uvw_zonebusiness_universe fzbMng (NOLOCK) on z.zone_group_id = fzbMng.zone_id and fzbMng.[type] = 'MANAGEDBY' and 
				(fzbMng.start_date<= @effective_date AND (fzbMng.end_date>= @effective_date OR fzbMng.end_date IS NULL))
		join uvw_zonebusiness_universe fzbOwn (NOLOCK)  on z.zone_group_id = fzbOwn.zone_id and fzbOwn.[type] = 'OWNEDBY' and 
				(fzbOwn.start_date<= @effective_date AND (fzbOwn.end_date>= @effective_date OR fzbOwn.end_date IS NULL))
		join uvw_zonedma_universe fzd (NOLOCK) on z.zone_group_id = fzd.zone_id
							and (fzd.start_date<=@effective_date AND (fzd.end_date>=@effective_date OR fzd.end_date IS NULL))
		join uvw_network_universe n  (NOLOCK) on z.network_id = n.network_id and n.active = 1 and
			(n.[start_date]<=@effective_date AND (n.end_date>=@effective_date OR n.end_date IS NULL))
	GROUP BY 
		n.network_id, n.code, fzbOwn.business_id , fzbMng.business_id, fzd.dma_id, z.zone_group_id

	select * from #coverage_universe_summary
END