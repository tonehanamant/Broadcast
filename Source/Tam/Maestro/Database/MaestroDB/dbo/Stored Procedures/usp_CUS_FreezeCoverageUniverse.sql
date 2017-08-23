
/***************************************************************************************************
** Date                    Author               Description   
** ---------  ----------           --------------------------------------------------------------------
** 08/20/2015 Abdul Sukkur  TFS 12690 - Freeze Coverage Universes at the Zone Level
** 10/13/2015 Abdul Sukkur  Included frozen_traffic_network_map & frozen_primary_subscribers tables
** 10/26/2015 Abdul Sukkur  Included zone group logic
** 05/10/2016 Abdul Sukkur  1885-Create table to store the proposal topography to mvpd topography mapping
** 05/16/2017	Abdul Sukkur	MME-1374-Custom Traffic SBTs will not affect MVPD's coverage universe
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_CUS_FreezeCoverageUniverse]
         @media_month_id int,
         @approved_user_id int,
         @approved_time datetime,
         @overwrite_data bit
AS
BEGIN

SET NOCOUNT ON;

DECLARE @effective_date_media_month AS DATETIME;
SELECT  @effective_date_media_month = [start_date] FROM media_months (NOLOCK) WHERE  id = @media_month_id

IF(@effective_date_media_month IS NULL)
       RETURN

IF(@overwrite_data = 1)
BEGIN
	EXEC [usp_CUS_Delete_frozen_coverage_universe_data] @media_month_id
END

INSERT INTO [frozen_media_months] ([media_month_id] , [effective_date], [approved_user_id], [approved_time]) 
VALUES (@media_month_id, @effective_date_media_month, @approved_user_id, @approved_time)


INSERT INTO [frozen_zones] ([media_month_id] , [id], [code], [name], [type], [primary], [traffic],[dma], [flag]) 
SELECT DISTINCT
              @media_month_id, z.[zone_id], z.[code], z.[name], z.[type], z.[primary], z.[traffic], z.[dma], z.[flag]
              FROM 
                     uvw_zone_universe z (NOLOCK)
              WHERE 
                     z.[active] = 1 
                     AND (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date] IS NULL)) 

INSERT INTO [frozen_networks] (   [media_month_id], [id], [code], [name], [flag], [language_id], [affiliated_network_id], [network_type_id]) 
SELECT DISTINCT
              @media_month_id, n.[network_id], n.[code], n.[name], n.[flag], n.[language_id], n.[affiliated_network_id], n.[network_type_id]
              FROM 
                     uvw_network_universe n (NOLOCK)
              WHERE 
                     n.[active] = 1 
                     AND (n.[start_date] <=@effective_date_media_month AND (n.[end_date] >=@effective_date_media_month OR n.[end_date] IS NULL)) 

                     
INSERT INTO [frozen_states] ( [media_month_id], [state_id], [code], [name], [flag])
SELECT DISTINCT
              @media_month_id, s.state_id , s.[code], s.[name], s.[flag]
              FROM 
                     [uvw_state_universe] s (NOLOCK)
              WHERE 
                     s.[active] = 1  AND
                     (s.[start_date] <=@effective_date_media_month AND (s.[end_date] >=@effective_date_media_month OR s.[end_date] IS NULL)) 


INSERT INTO [frozen_dmas] ( [media_month_id], [dma_id], [code], [name], [rank], [tv_hh], [cable_hh], [flag])
SELECT DISTINCT
              @media_month_id, d.[dma_id], d.[code], d.[name], d.[rank], d.[tv_hh], d.[cable_hh], d.[flag]
              FROM 
                     [uvw_dma_universe] d (NOLOCK)
              WHERE 
                     d.[active] = 1 AND
                     (d.[start_date] <=@effective_date_media_month AND (d.[end_date] >=@effective_date_media_month OR d.[end_date] IS NULL)) 

INSERT INTO [frozen_zone_states] ( [media_month_id] ,[zone_id] , [state_id] , [weight] )
SELECT DISTINCT
              @media_month_id, z.[zone_id], z.[state_id], z.[weight]
              FROM 
                     [uvw_zonestate_universe] z (NOLOCK)
              WHERE 
                     (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date] IS NULL)) 

INSERT INTO [frozen_zone_dmas] ( [media_month_id] ,[zone_id] , [dma_id] , [weight] )
SELECT DISTINCT
              @media_month_id, z.[zone_id], z.[dma_id], z.[weight]
              FROM 
                     [uvw_zonedma_universe] z (NOLOCK)
              WHERE 
                     (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date] IS NULL))       

INSERT INTO [frozen_zone_businesses] ( [media_month_id], [zone_id],  [business_id],       [type] )
SELECT DISTINCT
              @media_month_id, z.[zone_id], z.[business_id], z.[type]
              FROM 
                     [uvw_zonebusiness_universe] z (NOLOCK)
              WHERE 
                     (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date] IS NULL)) 

INSERT INTO [frozen_system_groups] ([media_month_id], [id], [name], [flag] )
SELECT DISTINCT
              @media_month_id, s.[system_group_id], s.[name], s.[flag]
              FROM 
                     [uvw_systemgroup_universe] s (NOLOCK)
              WHERE 
                     (s.[active] = 1 AND s.[start_date] <=@effective_date_media_month AND (s.[end_date] >=@effective_date_media_month OR s.[end_date] IS NULL)) 

INSERT INTO [frozen_system_zones] ([media_month_id], [zone_id], [system_id], [type])
SELECT DISTINCT
              @media_month_id, s.[zone_id], s.[system_id], s.[type]
              FROM 
                     [uvw_systemzone_universe] s (NOLOCK)
              WHERE 
                     (s.[start_date] <=@effective_date_media_month AND (s.[end_date] >=@effective_date_media_month OR s.[end_date] IS NULL)) 


INSERT INTO [frozen_systems] ( [media_month_id], [id] , [code] , [name] , [location] ,[spot_yield_weight],[traffic_order_format],[flag], [custom_traffic_system])
SELECT DISTINCT
              @media_month_id, s.[system_id], s.[code], s.[name], s.[location], s.[spot_yield_weight], s.[traffic_order_format], s.[flag], s.[custom_traffic_system]
              FROM 
                     [uvw_system_universe] s (NOLOCK)
              WHERE 
                     (s.[active] = 1 AND s.[start_date] <=@effective_date_media_month AND (s.[end_date] >=@effective_date_media_month OR s.[end_date] IS NULL)) 

INSERT INTO [frozen_businesses] ( [media_month_id],    [id], [code] ,[name] ,[type])
SELECT DISTINCT
              @media_month_id, b.[business_id], b.[code], b.[name], b.[type]
              FROM 
                     [uvw_business_universe] b (NOLOCK)
              WHERE 
                     (b.[active] = 1 AND b.[start_date] <=@effective_date_media_month AND (b.[end_date] >=@effective_date_media_month OR b.[end_date] IS NULL)) 

INSERT INTO [frozen_zone_networks] ([media_month_id] , [zone_id], [network_id] , [source], [trafficable], [primary], [subscribers] ) 
SELECT DISTINCT
              @media_month_id, zn.[zone_id],zn.[network_id],zn.[source],zn.[trafficable],zn.[primary],zn.[subscribers]
              FROM 
                     uvw_zonenetwork_universe zn (NOLOCK) 
                     JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id 
                     JOIN uvw_network_universe n (NOLOCK) ON n.network_id=zn.network_id 
              WHERE 
                     z.[active] = 1 
                     AND n.[active] = 1
                     AND (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date]  IS NULL)) 
                     AND (n.[start_date] <=@effective_date_media_month AND (n.[end_date] >=@effective_date_media_month OR n.[end_date]  IS NULL))
                     AND (zn.[start_date] <=@effective_date_media_month AND (zn.[end_date] >=@effective_date_media_month OR zn.[end_date]  IS NULL))
                     AND zn.subscribers > 0
                     AND zn.trafficable = 1

INSERT INTO [frozen_zone_zones] ( [media_month_id],    [primary_zone_id], [secondary_zone_id], [type])
SELECT DISTINCT
              @media_month_id, z.[primary_zone_id], z.[secondary_zone_id], z.[type]
              FROM 
                     [uvw_zonezone_universe] z (NOLOCK)
              WHERE 
                     (z.[start_date] <=@effective_date_media_month AND (z.[end_date] >=@effective_date_media_month OR z.[end_date] IS NULL)) 

                     
INSERT INTO [frozen_topographies] ([media_month_id], [id], [code], [name], [topography_type])
SELECT DISTINCT
              @media_month_id, t.[id], t.[code], t.[name], t.[topography_type]
              FROM 
                     [topographies] t (NOLOCK)


INSERT INTO [frozen_topography_businesses] ( [media_month_id], [topography_id], [business_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[business_id], t.[include]
              FROM 
                     [uvw_topography_business_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_topography_dmas] ( [media_month_id], [topography_id], [dma_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[dma_id], t.[include]
              FROM 
                     [uvw_topography_dma_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_topography_systems] ( [media_month_id], [topography_id], [system_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[system_id], t.[include]
              FROM 
                     [uvw_topography_system_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_topography_states] ( [media_month_id], [topography_id], [state_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[state_id], t.[include]
              FROM 
                     [uvw_topography_state_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_topography_zones] ( [media_month_id], [topography_id], [zone_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[zone_id], t.[include]
              FROM 
                     [uvw_topography_zone_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_system_group_systems] ( [media_month_id], [system_group_id], [system_id])
SELECT DISTINCT
              @media_month_id, s.[system_group_id], s.[system_id]
              FROM 
                     [uvw_systemgroupsystem_universe] s (NOLOCK)
              WHERE 
                     (s.[start_date] <=@effective_date_media_month AND (s.[end_date] >=@effective_date_media_month OR s.[end_date] IS NULL)) 

INSERT INTO [frozen_topography_system_groups] ( [media_month_id], [topography_id], [system_group_id], [include])
SELECT DISTINCT
              @media_month_id, t.[topography_id], t.[system_group_id], t.[include]
              FROM 
                     [uvw_topography_system_group_universe] t (NOLOCK)
              WHERE 
                     (t.[start_date] <=@effective_date_media_month AND (t.[end_date] >=@effective_date_media_month OR t.[end_date] IS NULL)) 

INSERT INTO [frozen_traffic_network_map] ( [media_month_id], [traffic_network_id], [zone_network_id])
SELECT DISTINCT
              @media_month_id,traffic_network_id, zone_network_id
              FROM 
                     dbo.udf_GetTrafficNetworkZoneNetworkMap(@effective_date_media_month)

--Get local Primary Subscribers First
-- Get all local zones that are not a zone group themselves
CREATE TABLE #z_subs (business_id int, zone_id int, network_id int, dma_id int, subscribers float);
INSERT INTO #z_subs(business_id, zone_id, network_id, dma_id, subscribers)
select 
                b.id [business_id], fz.id [zone_id], 
                           CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end as [network_id], 
                           d.dma_id [dma_id], MAX(fzn.subscribers) [subscribers]
from 
                frozen_zone_businesses fzb 
                join frozen_businesses b (NOLOCK) on b.id= fzb.business_id and  b.media_month_id= fzb.media_month_id
                join frozen_zones fz on fz.id = fzb.zone_id and fzb.[type] = 'MANAGEDBY' and fz.media_month_id = fzb.media_month_id 
                join frozen_zone_networks fzn (NOLOCK) on fzn.zone_id = fz.id and fzn.media_month_id = fzb.media_month_id and fz.id in
                    (SELECT fsz.zone_id FROM frozen_system_zones fsz (NOLOCK)   
                                  JOIN frozen_systems fs (NOLOCK) ON fs.id = fsz.system_id and fs.media_month_id = fsz.media_month_id 
                                  WHERE fsz.media_month_id = @media_month_id  and fs.custom_traffic_system = 0 and fsz.[type] = 'TRAFFIC') 
                join frozen_zone_dmas fzd (NOLOCK) on fzd.zone_id = fz.id and fzd.media_month_id = fzb.media_month_id
                join frozen_dmas d (NOLOCK) on d.dma_id = fzd.dma_id and d.media_month_id = fzb.media_month_id 
                join frozen_networks n (NOLOCK) on n.id = fzn.network_id and n.media_month_id = fzb.media_month_id
                           left join frozen_traffic_network_map rs  (NOLOCK)  on rs.zone_network_id = fzn.network_id and rs.media_month_id = fzn.media_month_id
where
                fzb.media_month_id = @media_month_id
                and fz.[type] not in('Zone Group')
                and fz.[primary] = 1
                and fz.traffic = 1 
                and fzn.[primary] = 1
                and fzn.trafficable = 1
GROUP BY
                b.id , fz.id , CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end, d.dma_id  
             
--Zone group
-- Get trafficable zone groups
CREATE TABLE #traffic_zone_groups(primary_zone_id int, secondary_zone_id int);
INSERT INTO #traffic_zone_groups (primary_zone_id,secondary_zone_id) 
select distinct ozg.primary_zone_id, ozg.secondary_zone_id 
from 
                frozen_zone_zones ozg 
                join frozen_zones zg on ozg.primary_zone_id = zg.id and  ozg.media_month_id = zg.media_month_id 
                join frozen_system_zones szu on szu.zone_id = zg.id and szu.[type] = 'TRAFFIC'  and szu.media_month_id = zg.media_month_id
                join frozen_systems s (NOLOCK) ON s.id = szu.system_id and s.media_month_id = szu.media_month_id and s.custom_traffic_system = 0   
                join frozen_zones lz on ozg.secondary_zone_id = lz.id  and ozg.media_month_id = lz.media_month_id 
                where
                    zg.[type] = 'Zone Group'
                    and zg.traffic = 1 
                    and lz.[primary] = 1
                    and  ozg.media_month_id = @media_month_id ;

-- FIRST, GET ALL SUBS FOR A ZONE GROUP THAT HAS NOT BEEN COUNTED IN STEP 1 AND IS NOT IN ANOTHER TRAFFICABLE ZONE GROUP
CREATE TABLE #zg_subs (zone_group_id int, zone_id int, network_id int, subscribers float);
INSERT INTO #zg_subs (zone_group_id, zone_id, network_id, subscribers)
select 
                zg.id [zone_group_id],
                fz.id [zone_id], 
                CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end as [network_id],
                MAX(fzn.subscribers) [subscribers]
from 
                frozen_zone_businesses fzb 
                join frozen_businesses b (NOLOCK) on b.id= fzb.business_id and b.media_month_id= fzb.media_month_id
                join #traffic_zone_groups tzg on fzb.zone_id = tzg.primary_zone_id 
                join frozen_zones zg (NOLOCK) on zg.id = fzb.zone_id and fzb.[type] = 'MANAGEDBY' and zg.media_month_id= fzb.media_month_id
                join frozen_zones fz on fz.id = tzg.secondary_zone_id and  fz.media_month_id= fzb.media_month_id --local zone join here
                                and fz.id not in (select distinct zone_id from #z_subs) --makes sure it wasn't already counted as a straight local
                                and fz.id not in (select distinct secondary_zone_id from #traffic_zone_groups where primary_zone_id <> zg.id) --make sure its not in another trafficable zone group
                join frozen_zone_networks fzn (NOLOCK) on fzn.zone_id = fz.id and  fzn.media_month_id= fzb.media_month_id  --local networks
                join frozen_zone_dmas fzd (NOLOCK) on fzd.zone_id = zg.id and  fzd.media_month_id= fzb.media_month_id
                join frozen_dmas d (NOLOCK) on d.dma_id = fzd.dma_id  and  d.media_month_id= fzb.media_month_id
                join frozen_networks n (NOLOCK) on n.id = fzn.network_id  and  n.media_month_id= fzb.media_month_id
                left join frozen_traffic_network_map rs  (NOLOCK)  on rs.zone_network_id = fzn.network_id and rs.media_month_id = fzn.media_month_id              
where
                b.media_month_id = @media_month_id
                and zg.[type] in('Zone Group')
                and fz.[primary] = 1
                and zg.traffic = 1 
                and fzn.[primary] = 1
                and fzn.trafficable = 1
GROUP BY
                zg.id, fz.id, CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end

-- GET SUBS FOR ALL LOCALS IN A ZONE GROUP THAT BELONG TO MULTIPLE ACTIVE ZONE GROUPS
INSERT INTO #zg_subs (zone_group_id, zone_id, network_id, subscribers)
select 
                zg.id [zone_group_id],
                fz.id [zone_id], 
                CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end as [network_id], 
                MAX(fzn.subscribers) [subscribers]
from 
                           frozen_zone_businesses fzb 
                join frozen_businesses b (NOLOCK) on b.id= fzb.business_id and b.media_month_id= fzb.media_month_id
                join #traffic_zone_groups tzg on fzb.zone_id = tzg.primary_zone_id
                join frozen_zones zg (NOLOCK) on zg.id = fzb.zone_id and fzb.[type] = 'MANAGEDBY'  and zg.media_month_id= fzb.media_month_id
                join frozen_zones fz on fz.id = tzg.secondary_zone_id  and fz.media_month_id= fzb.media_month_id --local zone join here
                                                       and fz.id not in (select distinct zone_id from #z_subs) --makes sure it wasn't already counted as a straight local
                                and fz.id in (select distinct secondary_zone_id from #traffic_zone_groups where primary_zone_id <> zg.id) --make sure its not in another trafficable zone group
                -- because it is in multiple zone groups, check to match the managed by flags
                join frozen_zone_businesses fzbl (NOLOCK) on fzbl.zone_id = fz.id and fzbl.type = 'MANAGEDBY' and fzbl.business_id = fzb.business_id  and fzbl.media_month_id= fzb.media_month_id  
                join frozen_zone_networks fzn (NOLOCK) on fzn.zone_id = fz.id  and fzn.media_month_id= fzb.media_month_id --local networks
                join frozen_zone_dmas fzd (NOLOCK) on fzd.zone_id = zg.id  and fzd.media_month_id= fzb.media_month_id
                join frozen_dmas d (NOLOCK) on d.dma_id = fzd.dma_id and  d.media_month_id= fzb.media_month_id
                join frozen_networks n (NOLOCK) on n.id = fzn.network_id and n.media_month_id= fzb.media_month_id
                left join frozen_traffic_network_map rs  (NOLOCK) on rs.zone_network_id = fzn.network_id and rs.media_month_id = fzn.media_month_id
where
                fzb.media_month_id = @media_month_id
                and zg.[type] in('Zone Group')
                and fz.[primary] = 1
                and zg.traffic = 1 
                and fzn.[primary] = 1
                and fzn.trafficable = 1
GROUP BY
                zg.id, fz.id, CASE WHEN rs.traffic_network_id is null then  n.id else rs.traffic_network_id end

-- insert into frozen table
INSERT INTO [frozen_primary_subscribers] ( [media_month_id], [network_id], [managed_business_id], [dma_id],[zone_id], [subscribers])
SELECT @media_month_id,network_id, business_id, dma_id, zone_id, subscribers from #z_subs;

INSERT INTO [frozen_primary_subscribers] ( [media_month_id], [network_id], [managed_business_id], [dma_id],[zone_id], [subscribers])
SELECT @media_month_id,subs.network_id, b.id, fzd.dma_id, fzb.zone_id, SUM(subs.subscribers) as subscribers
from 
                frozen_businesses b (NOLOCK) 
                join frozen_zone_businesses fzb (NOLOCK) on b.id= fzb.business_id and fzb.type = 'MANAGEDBY' and b.media_month_id= fzb.media_month_id  
                join #zg_subs subs on subs.zone_group_id = fzb.zone_id
                join frozen_zone_dmas fzd (NOLOCK) on fzd.zone_id = fzb.zone_id and fzd.media_month_id= fzb.media_month_id  
WHERE
                b.media_month_id = @media_month_id
GROUP BY 
                     b.id, fzb.zone_id , subs.network_id, fzd.dma_id

                
drop table #z_subs
drop table #zg_subs;
drop table #traffic_zone_groups;

--Freeze frozen_proposal_mvpd_topography_map

Declare @proposal_topography_id INT 
  
DECLARE TopographyCursor CURSOR FAST_FORWARD FOR
	SELECT distinct ft.media_month_id, ft.id FROM frozen_topographies ft where ft.topography_type in (0,1) and ft.media_month_id = @media_month_id

OPEN TopographyCursor
FETCH NEXT FROM TopographyCursor INTO @media_month_id, @proposal_topography_id
WHILE @@FETCH_STATUS = 0
	BEGIN

		SELECT DISTINCT z.id AS id 
		INTO #zoneTable
		FROM topographies t (NOLOCK) 
		LEFT OUTER JOIN frozen_topography_systems ts (NOLOCK) on ts.topography_id = t.id 
			AND ts.include = 1 
			AND ts.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_topography_zones tz (NOLOCK) on tz.topography_id = t.id 
			AND tz.include = 1
			AND tz.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_topography_system_groups tsg (NOLOCK) on tsg.topography_id = t.id 
			AND tsg.include = 1
			AND tsg.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_system_groups sg (NOLOCK) on sg.id = tsg.system_group_id
			AND sg.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_system_group_systems sgs (NOLOCK) on sgs.system_group_id = tsg.system_group_id 
			AND sgs.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_system_zones sz (NOLOCK) on (sz.system_id = sgs.system_id OR sz.system_id = ts.system_id)
			AND sz.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_systems (NOLOCK) s on (s.id = ts.system_id OR s.id = sz.system_id)
			AND s.media_month_id = @media_month_id
		LEFT OUTER JOIN frozen_zones z (NOLOCK) on (z.id = tz.zone_id OR sz.zone_id = z.id) 
			AND z.traffic = 1 
			AND z.media_month_id = @media_month_id
		WHERE t.id = @proposal_topography_id

		INSERT INTO frozen_proposal_mvpd_topography_map (media_month_id,proposal_topography_id, mvpd_topography_id, mvpd_business_id) 
		SELECT DISTINCT @media_month_id , @proposal_topography_id, ft.id as mvpd_topography_id, fzb.business_id as mvpd_business_id
		FROM frozen_zone_businesses fzb (NOLOCK) 
		INNER JOIN frozen_topography_businesses ftb (NOLOCK) ON fzb.business_id = ftb.business_id 
			AND ftb.media_month_id = @media_month_id 
			AND ftb.include = 1
		INNER JOIN frozen_topographies ft (NOLOCK) on ft.id = ftb.topography_id
			AND ft.media_month_id = @media_month_id 
		WHERE ft.topography_type = 2
			AND fzb.media_month_id = @media_month_id
			AND fzb.type = 'MANAGEDBY'
			AND fzb.zone_id IN (
					SELECT id 
					FROM #zoneTable)

		Drop table #zoneTable
		FETCH NEXT FROM TopographyCursor INTO @media_month_id, @proposal_topography_id
	END
CLOSE TopographyCursor
DEALLOCATE TopographyCursor	

END