
/***************************************************************************************************
** Date			Author			Description	
** ---------	----------		--------------------------------------------------------------------
** 05/15/2015	Abdul Sukkur	Get coverage universe calculation for network details
** 12/08/2015	Abdul Sukkur	Included traffic system logic
** 05/16/2017	Abdul Sukkur	MME-1374-Custom Traffic SBTs will not affect MVPD's coverage universe
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_CUS_GetCoverageUniversesForNetworkDetails]
	  @network_id int,
      @media_month1 varchar(15),
	  @media_month2 varchar(15)
AS
BEGIN

SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  
SET NOCOUNT ON;

DECLARE @media_month1_id AS INT;
DECLARE @effective_date_media_month1 AS DATETIME;
SELECT @media_month1_id = id , @effective_date_media_month1 = [start_date] 
	FROM media_months (NOLOCK) WHERE media_month = @media_month1

DECLARE @media_month2_id AS INT;
DECLARE @effective_date_media_month2 AS DATETIME;
SELECT @media_month2_id = id , @effective_date_media_month2 = [start_date] 
	FROM media_months (NOLOCK) WHERE media_month = @media_month2

declare @NetWorkList1 table (NetworkId int)

Insert into @NetWorkList1 
Select distinct zone_network_id 
from udf_GetTrafficNetworkZoneNetworkMap(@effective_date_media_month1)  where traffic_network_id = @network_id
insert into @NetWorkList1 values (@network_id)


declare @NetWorkList2 table (NetworkId int)

Insert into @NetWorkList2 
Select distinct zone_network_id 
from udf_GetTrafficNetworkZoneNetworkMap(@effective_date_media_month2)  where traffic_network_id = @network_id
insert into @NetWorkList2 values (@network_id)

--get subscribers count for current/previous month 
SELECT
			zn.network_id,
			n.code as network_code,
			z.zone_id,
			z.[primary] as ZonePrimary,
			zn.[primary]  as ZoneNetworkPrimary,
			z.end_date as ZoneEndDate,
			zn.end_date as ZoneNetworkEndDate,
			z.name,
			z.[type] as ZoneType,
			z.[code] as [SysCode],
			CAST(MAX(zn.subscribers) AS FLOAT) [month1_subscribers],
			CAST(0 AS FLOAT) as [month2_subscribers] 
			INTO #ZONE_LIST
		FROM 
			uvw_zonenetwork_universe zn (NOLOCK) 
			JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id 
			JOIN uvw_network_universe n (NOLOCK) ON n.network_id=zn.network_id 
		WHERE 
		    n.network_id in (select NetworkId from @NetWorkList1)
			AND z.[active] = 1 
		--	AND z.[primary] = 1 
			AND n.[active] = 1
		--	AND zn.[primary] = 1 
			AND z.[traffic] = 1 
			AND zn.[trafficable] = 1 
			AND (z.[start_date] <=@effective_date_media_month1 AND (z.[end_date] >=@effective_date_media_month1 OR z.[end_date]  IS NULL)) 
			AND (n.[start_date] <=@effective_date_media_month1 AND (n.[end_date] >=@effective_date_media_month1 OR n.[end_date]  IS NULL))
			AND (zn.[start_date] <=@effective_date_media_month1 AND (zn.[end_date] >=@effective_date_media_month1 OR zn.[end_date]  IS NULL))
			AND z.zone_id in 
					(SELECT sz.zone_id FROM uvw_systemzone_universe sz (NOLOCK)   
                     JOIN uvw_system_universe s (NOLOCK) ON s.system_id = sz.system_id 
                     and (s.[start_date] <=@effective_date_media_month1 AND (s.[end_date]>=@effective_date_media_month1 OR s.[end_date] IS NULL))
                    WHERE (sz.[start_date] <=@effective_date_media_month1 AND (sz.[end_date]>=@effective_date_media_month1 OR sz.[end_date] IS NULL)) 
						AND s.active = 1 and s.custom_traffic_system = 0 AND sz.[type] = 'TRAFFIC')
			GROUP BY zn.network_id, n.code, z.zone_id, z.[primary],zn.[primary],z.end_date, zn.end_date, z.name, z.[type],z.[code];

SELECT
			zn.network_id,
			n.code as network_code,
			z.zone_id,
			z.name,
			z.[type] as ZoneType,
			z.[code] as [SysCode],
			CAST(0 AS FLOAT) as [month1_subscribers] ,
			CAST(MAX(zn.subscribers) AS FLOAT) [month2_subscribers] 
		INTO #MONTH2_SUBS
		FROM 
			uvw_zonenetwork_universe zn (NOLOCK) 
			JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id 
			JOIN uvw_network_universe n (NOLOCK) ON n.network_id=zn.network_id 
		WHERE 
			n.network_id in (select NetworkId from @NetWorkList2)
			AND z.[active] = 1 
			--AND z.[primary] = 1 
			AND n.[active] = 1
			--AND zn.[primary] = 1 
			AND z.[traffic] = 1 
			AND zn.[trafficable] = 1 
			AND (z.[start_date] <=@effective_date_media_month2 AND (z.[end_date] >=@effective_date_media_month2 OR z.[end_date]  IS NULL)) 
			AND (n.[start_date] <=@effective_date_media_month2 AND (n.[end_date] >=@effective_date_media_month2 OR n.[end_date]  IS NULL))
			AND (zn.[start_date] <=@effective_date_media_month2 AND (zn.[end_date] >=@effective_date_media_month2 OR zn.[end_date]  IS NULL))
			AND z.zone_id in 
				(SELECT sz.zone_id FROM uvw_systemzone_universe sz (NOLOCK)   
                    JOIN uvw_system_universe s (NOLOCK) ON s.system_id = sz.system_id 
                    and (s.[start_date] <=@effective_date_media_month2 AND (s.[end_date]>=@effective_date_media_month2 OR s.[end_date] IS NULL))
                WHERE (sz.[start_date] <=@effective_date_media_month2 AND (sz.[end_date]>=@effective_date_media_month2 OR sz.[end_date] IS NULL)) 
					AND s.active = 1 and s.custom_traffic_system = 0 AND sz.[type] = 'TRAFFIC')
			GROUP BY zn.network_id, n.code, z.zone_id, z.name, z.[type],z.[code];

MERGE #ZONE_LIST AS Target
USING #MONTH2_SUBS AS Source
	ON (Target.zone_id = Source.zone_id and Target.network_id = Source.network_id)
WHEN MATCHED THEN
	Update Set Target.[month2_subscribers] = Source.[month2_subscribers]
WHEN NOT MATCHED BY Target THEN
	INSERT (network_id, network_code, zone_id, ZonePrimary,ZoneNetworkPrimary,name,ZoneType,SysCode,month1_subscribers,month2_subscribers) 
	VALUES (Source.network_id, Source.network_code, Source.zone_id, 0, 0, Source.name, Source.ZoneType, Source.SysCode, Source.month1_subscribers, Source.month2_subscribers);

-- calculate the total subscribers
SELECT  distinct
		    zl.network_id  as [ZoneNetworkId],
			zl.network_code as  [ZoneNetworkCode],
			zl.zone_id  as [ZoneId],
			b.name as [MvpdName],
			d.name as [MarketName],
			zl.name as [ZoneName],
			zl.ZoneType as [ZoneType],
			zl.[SysCode] as [SysCode],
			zl.ZonePrimary as [IsZonePrimary],
			zl.ZoneNetworkPrimary as [IsZoneNetworkPrimary],
			zl.ZoneEndDate as [ZoneEndDate],
			zl.ZoneNetworkEndDate as [ZoneNetworkEndDate],
			ISNULL([month1_subscribers],0) as [TotalMonth1Subscribers],
			ISNULL([month2_subscribers],0) as [TotalMonth2Subscribers]
		FROM 
			#ZONE_LIST zl (NOLOCK) 
			LEFT JOIN uvw_zonebusiness_universe zb (NOLOCK) ON zb.zone_id = zl.zone_id AND zb.type='MANAGEDBY' 
				AND zb.start_date<=@effective_date_media_month1 AND (zb.end_date>=@effective_date_media_month1 OR zb.end_date IS NULL)
			LEFT JOIN uvw_business_universe b (NOLOCK) ON b.business_id=zb.business_id
				AND b.start_date<=@effective_date_media_month1 AND (b.end_date>=@effective_date_media_month1 OR b.end_date IS NULL)
			LEFT JOIN uvw_zonedma_universe zd (NOLOCK) ON zd.zone_id = zl.zone_id 
				AND zd.start_date<=@effective_date_media_month1 AND (zd.end_date>=@effective_date_media_month1 OR zd.end_date IS NULL)
			LEFT JOIN uvw_dma_universe d (NOLOCK) ON d.dma_id= zd.dma_id 
				AND d.start_date<=@effective_date_media_month1 AND (d.end_date>=@effective_date_media_month1 OR d.end_date IS NULL)

END