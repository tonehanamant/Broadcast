/***************************************************************************************************
** Date			Author			Description	
** ---------	------------	-------------------------------------------------------------------
** 12/08/2015	Abdul Sukkur	Get universe data for network, business, dma, zone
** 01/06/2016	Abdul Sukkur	changed regional network logic to take only one zone network if more than one zone network found for the same zone
** 05/16/2017	Abdul Sukkur	MME-1374-Custom Traffic SBTs will not affect MVPD's coverage universe
** 05/16/2017	Abdul Sukkur	MME-3261-Coverage Universe - Network goes missing when Subs drop to 0 for current month
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_CUS_GetCoverageUniverseOverviewData]
      @media_month1 varchar(15),
	  @media_month2 varchar(15)
AS
BEGIN


SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;  
SET NOCOUNT ON;

DECLARE @media_month1_id AS INT;
DECLARE @effective_date_media_month1 AS DATETIME;
SELECT @media_month1_id = id , @effective_date_media_month1 = [start_date] 
	FROM media_months (NOLOCK) WHERE media_month =@media_month1

DECLARE @media_month2_id AS INT;
DECLARE @effective_date_media_month2 AS DATETIME;
SELECT @media_month2_id = id , @effective_date_media_month2 = [start_date] 
	FROM media_months (NOLOCK) WHERE media_month =@media_month2

DECLARE @media_month1_Year AS INT;
DECLARE @media_month1_Quarter AS INT;

SET @media_month1_Year =  DATEPART(YYYY, @effective_date_media_month1);
SET @media_month1_Quarter =  DATEPART(Q, @effective_date_media_month1);


CREATE TABLE #media_month1_universe (media_month_id int, network_id int,  network_code  varchar(15), owned_business_id int, managed_business_id int, dma_id int, zone_id int, subscribers float);
CREATE TABLE #media_month2_universe (media_month_id int, network_id int,  network_code  varchar(15), owned_business_id int, managed_business_id int, dma_id int, zone_id int, subscribers float);

IF((select count(1) from frozen_primary_subscribers s where s.media_month_id = @media_month1_id) > 0)
BEGIN
		INSERT INTO #media_month1_universe(media_month_id, network_id, network_code, owned_business_id, managed_business_id, dma_id, zone_id, subscribers)
			SELECT @media_month1_id, n.network_id, n.code, fzbOwn.business_id as owned_business_id, z.managed_business_id as managed_business_id, 
				dma_id, z.zone_id, MAX(subscribers) as  subscribers 
			from frozen_primary_subscribers as z
		join uvw_zonebusiness_universe fzbOwn (NOLOCK) on z.zone_id= fzbOwn.zone_id and fzbOwn.type = 'OWNEDBY' and 
			(fzbOwn.[start_date]<=@effective_date_media_month1 AND (fzbOwn.end_date>=@effective_date_media_month1 OR fzbOwn.end_date IS NULL))
		left join frozen_traffic_network_map  rs  (NOLOCK) on rs.zone_network_id = z.network_id    and rs.media_month_id =  @media_month1_id
		join uvw_network_universe n  (NOLOCK) on ISNULL(rs.traffic_network_id , z.network_id) = n.network_id and
			(n.[start_date]<=@effective_date_media_month1 AND (n.end_date>=@effective_date_media_month1 OR n.end_date IS NULL))
		where z.media_month_id = @media_month1_id
		group by z.media_month_id, n.network_id, n.code, fzbOwn.business_id, z.managed_business_id,  dma_id, z.zone_id
END
ELSE
BEGIN
	INSERT INTO #media_month1_universe EXEC usp_CUS_GetCoverageUniverseSummaryData @media_month1_id;
END

IF((select count(1) from frozen_primary_subscribers s where s.media_month_id = @media_month2_id) > 0)
BEGIN
		INSERT INTO #media_month2_universe(media_month_id, network_id, network_code, owned_business_id, managed_business_id, dma_id, zone_id, subscribers)
			SELECT @media_month2_id, n.network_id, n.code, fzbOwn.business_id as owned_business_id, z.managed_business_id as managed_business_id, 
				dma_id, z.zone_id, MAX(subscribers) as  subscribers 
			from frozen_primary_subscribers as z
		join uvw_zonebusiness_universe fzbOwn (NOLOCK) on z.zone_id= fzbOwn.zone_id and fzbOwn.type = 'OWNEDBY' and 
			(fzbOwn.[start_date]<=@effective_date_media_month2 AND (fzbOwn.end_date>=@effective_date_media_month2 OR fzbOwn.end_date IS NULL))
		left join frozen_traffic_network_map rs  (NOLOCK) on rs.zone_network_id = z.network_id   and rs.media_month_id =  @media_month2_id
		join uvw_network_universe n  (NOLOCK) on ISNULL(rs.traffic_network_id , z.network_id) = n.network_id and
			(n.[start_date]<=@effective_date_media_month2 AND (n.end_date>=@effective_date_media_month2 OR n.end_date IS NULL))
		where z.media_month_id = @media_month2_id
		group by z.media_month_id, n.network_id, n.code, fzbOwn.business_id, z.managed_business_id,  dma_id, z.zone_id
END
ELSE
BEGIN
	INSERT INTO #media_month2_universe EXEC usp_CUS_GetCoverageUniverseSummaryData @media_month2_id;
END


--get subscribers count for current/previous month per network
SELECT
			n.network_id,
			n.network_code,
			CAST(SUM(n.subscribers) AS FLOAT) [month1_subscribers],
			CAST(0 AS FLOAT) as [month2_subscribers] 
		INTO #NW_LIST
		FROM 
			#media_month1_universe n 
		group by n.[network_id], n.[network_code];

SELECT
			n.network_id,
			n.network_code,
			CAST(0 AS FLOAT) as [month1_subscribers] ,
			CAST(SUM(n.subscribers) AS FLOAT) [month2_subscribers] 
	    INTO #PREV_MONTH_SUBS_NW
		FROM 
			#media_month2_universe n 
		group by n.[network_id], n.[network_code];

MERGE #NW_LIST AS Target
USING #PREV_MONTH_SUBS_NW AS Source
	ON (Target.network_id = Source.network_id)
WHEN MATCHED THEN
	Update Set Target.[month2_subscribers] = Source.[month2_subscribers]
WHEN NOT MATCHED BY Target THEN
	INSERT VALUES (Source.network_id, Source.network_code, Source.[month1_subscribers], Source.[month2_subscribers]);
	

SELECT nwl.network_id,nwtier.tier 
INTO #NW_TIER
FROM  #NW_LIST  (NOLOCK) nwl 
CROSS APPLY (
select 
		TOP 1 nrcd.tier
	from 
		network_rate_card_details nrcd with(nolock) 
		join network_rate_cards nrc with(nolock) on 
			nrc.id=nrcd.network_rate_card_id
		join network_rate_card_books nrcb with(nolock) on 
			nrcb.id=nrc.network_rate_card_book_id
	where nrcd.network_id = nwl.network_id AND [year] = @media_month1_Year and [quarter] = @media_month1_Quarter
	order by date_approved desc , [version] desc) as nwtier

-- Get the result
select distinct nwl.network_id as [NetworkId], nwl.network_code as [Name], cast(nwtier.tier AS varchar(10)) as [RateCard],
	ISNULL(nwl.month1_subscribers,0) as [TotalMonth1Subscribers], 
	ISNULL(nwl.[month2_subscribers],0) as [TotalMonth2Subscribers],
	@media_month1_id as [MediaMonthId],
	cucn.id as [ReviewId],
	cucn.reviewed_user_id as [ReviewedByUserId],
	emp.username as [ReviewedBy],
	cucn.reviewed_time as [ReviewedTime]
from #NW_LIST  nwl
LEFT JOIN #NW_TIER nwtier (NOLOCK) on nwtier.network_id = nwl.network_id
LEFT JOIN coverage_universe_comparison_network cucn (NOLOCK) on cucn.media_month_id = @media_month1_id AND cucn.network_id = nwl.network_id
LEFT JOIN employees emp (NOLOCK) on emp.id = cucn.reviewed_user_id
order by nwl.network_id

--network zone types

	SELECT
			zn.network_id  as [NetworkId],
			z.type as [ZoneType],
			CAST(SUM(zn.subscribers) AS FLOAT)  as [TotalMonth1Subscribers]
		FROM 
			#media_month1_universe zn  
			JOIN uvw_zone_universe z (NOLOCK) ON z.zone_id=zn.zone_id 
		WHERE 
			z.[active] = 1 
			AND z.[primary] = 1 
			AND (z.[start_date] <=@effective_date_media_month1 AND (z.[end_date] >=@effective_date_media_month1 OR z.[end_date]  IS NULL)) 
			group by zn.[network_id], z.[type]

--Market

--get subscribers count for month1/month2 per market with highest subsc for networks
SELECT		dma_id,
			[name],
			[rank],
			network_id,
			network_name,
			CAST(0 AS int) as month2_network_id,
			CAST('' AS varchar(15)) month2_network_name,
			SUM([month1_subscribers]) as [month1_subscribers] ,
			CAST(0 AS FLOAT) as [month2_subscribers] 
			INTO #MKT_LIST
			FROM ( 
		SELECT
			d.dma_id,
			d.name,
			d.[rank],
			n.zone_id,
			n.network_id,
			n.network_code as network_name,
			CAST(MAX(n.subscribers) AS FLOAT) [month1_subscribers]
		FROM 
			#media_month1_universe n 
			JOIN uvw_zonedma_universe zd (NOLOCK) ON n.dma_id=zd.dma_id
			JOIN uvw_dma_universe d  (NOLOCK) ON d.dma_id=zd.dma_id
		WHERE 
			d.[active] = 1 
			AND (d.[start_date] <=@effective_date_media_month1 AND (d.[end_date] >=@effective_date_media_month1 OR d.[end_date]  IS NULL))
			AND (zd.[start_date] <=@effective_date_media_month1 AND (zd.[end_date] >=@effective_date_media_month1 OR zd.[end_date]  IS NULL))
			GROUP BY d.dma_id, d.name, d.[rank], n.zone_id,n.network_id, n.network_code) AS A 
			GROUP BY dma_id, name, [rank],network_id,network_name

SELECT		dma_id,
			[name],
			[rank],
			network_id as month2_network_id,
			network_name as month2_network_name,
			CAST(0 AS FLOAT) as [month1_subscribers] ,
			SUM([month2_subscribers]) as [month2_subscribers] 
			INTO #MONTH2_SUBS_MKT
			FROM ( 
		SELECT
			d.dma_id,
			d.name,
			d.[rank],
			n.zone_id,
			n.network_id,
			n.network_code as network_name,
			CAST(MAX(n.subscribers) AS FLOAT) [month2_subscribers] 
		FROM 
			#media_month2_universe n 
			JOIN uvw_zonedma_universe zd (NOLOCK) ON n.dma_id=zd.dma_id
			JOIN uvw_dma_universe d  (NOLOCK) ON d.dma_id=zd.dma_id
		WHERE 
			d.[active] = 1 
			AND (d.[start_date] <=@effective_date_media_month2 AND (d.[end_date] >=@effective_date_media_month2 OR d.[end_date]  IS NULL))
			AND (zd.[start_date] <=@effective_date_media_month2 AND (zd.[end_date] >=@effective_date_media_month2 OR zd.[end_date]  IS NULL))
			GROUP BY d.dma_id, d.name, d.[rank],n.zone_id,n.network_id, n.network_code) AS C 
			GROUP BY dma_id, name, [rank],network_id,network_name

MERGE #MKT_LIST AS Target
USING #MONTH2_SUBS_MKT AS Source
	ON (Target.dma_id = Source.dma_id and Target.network_id = Source.month2_network_id)
WHEN MATCHED THEN
	Update Set Target.[month2_subscribers] = Source.[month2_subscribers], Target.month2_network_id = Source.month2_network_id, Target.month2_network_name = Source.month2_network_name
WHEN NOT MATCHED BY Target THEN
	INSERT VALUES (Source.dma_id, Source.name, Source.[rank], Source.month2_network_id,Source.month2_network_name, Source.month2_network_id,Source.month2_network_name, Source.[month1_subscribers], Source.[month2_subscribers]);

-- Get the result
select distinct mkl.dma_id as MarketId, 
	mkl.name as Name, 
	mkl.[rank] as [Rank], 
	network_id as NetworkId,
	network_name as NetworkName,
	month2_network_id as Month2NetworkId,
	month2_network_name as Month2NetworkName,
	ISNULL(mkl.month1_subscribers,0) as [TotalMonth1Subscribers], 
	ISNULL(mkl.[month2_subscribers],0) as [TotalMonth2Subscribers],
	@media_month1_id as [MediaMonthId],
	cucn.id as [ReviewId],
	cucn.reviewed_user_id as [ReviewedByUserId],
	emp.username as [ReviewedBy],
	cucn.reviewed_time as [ReviewedTime]
from #MKT_LIST  mkl
LEFT JOIN coverage_universe_comparison_market cucn (NOLOCK) on cucn.media_month_id = @media_month1_id AND cucn.market_id = mkl.dma_id
LEFT JOIN employees emp (NOLOCK) on emp.id = cucn.reviewed_user_id

-- mvpd

--get subscribers count for month1/month2 per network/zone type
SELECT business_id,
			name,
			network_id,
			network_name,
			CAST(0 AS int) as month2_network_id,
			CAST('' AS varchar(15)) month2_network_name,
			SUM([month1_subscribers]) as [month1_subscribers],
			CAST(0 AS FLOAT) as [month2_subscribers]
			INTO #MVPD_LIST
			FROM ( 
		SELECT
			b.business_id,
			b.name as name,
			n.zone_id,
			n.network_id,
			n.network_code as network_name,
			CAST(MAX(n.subscribers) AS FLOAT) [month1_subscribers]
		FROM 
			#media_month1_universe n 
			JOIN uvw_business_universe b (NOLOCK) ON b.business_id=n.managed_business_id
		WHERE 
			b.[active] = 1 
			AND (b.[start_date] <=@effective_date_media_month1 AND (b.[end_date] >=@effective_date_media_month1 OR b.[end_date]  IS NULL))
			GROUP BY  b.business_id, b.name, n.zone_id, n.network_id, n.network_code, n.dma_id) AS A
			GROUP BY business_id, name, network_id, network_name

	SELECT business_id,
		name,
		network_id as month2_network_id,
		network_name as month2_network_name,
		CAST(0 AS FLOAT) as [month1_subscribers] ,
		SUM([month2_subscribers]) as [month2_subscribers] 
	INTO #MONTH2_SUBS_MVPD
	FROM ( 
		SELECT
			b.business_id,
			b.name as name,
			n.zone_id,
			n.network_id,
			n.network_code as network_name,
			CAST(MAX(n.subscribers) AS FLOAT) [month2_subscribers] 
		FROM 
			#media_month2_universe n 
			JOIN uvw_business_universe b (NOLOCK) ON b.business_id=n.managed_business_id
		WHERE 
			b.[active] = 1 
		AND (b.[start_date] <=@effective_date_media_month1 AND (b.[end_date] >=@effective_date_media_month1 OR b.[end_date]  IS NULL))
			GROUP BY b.business_id, b.name, n.zone_id, n.network_id, n.network_code, n.dma_id) AS C
			GROUP BY business_id, name, network_id, network_name

MERGE #MVPD_LIST AS Target
USING #MONTH2_SUBS_MVPD AS Source
	ON (Target.business_id = Source.business_id and Target.network_id = Source.month2_network_id)
WHEN MATCHED THEN
	Update Set Target.[month2_subscribers] = Source.[month2_subscribers], Target.month2_network_id = Source.month2_network_id, Target.month2_network_name = Source.month2_network_name
WHEN NOT MATCHED BY Target THEN
	INSERT VALUES (Source.business_id, Source.name, Source.month2_network_id,Source.month2_network_name,Source.month2_network_id,Source.month2_network_name, Source.[month1_subscribers], Source.[month2_subscribers]);

--Get Owned subscribers count
SELECT
		business_id,
		network_id,
		CAST(SUM(subscribers) AS FLOAT)  as owned_subscribers
INTO #OWNED_BY
FROM (
SELECT
		n.owned_business_id as business_id,
		n.zone_id,
		n.network_id,
		CAST(MAX(n.subscribers) AS FLOAT)  as subscribers
FROM
	#media_month1_universe n 
WHERE 
	n.owned_business_id = n.managed_business_id
group by n.owned_business_id ,n.zone_id, n.network_id, n.dma_id ) AS E
group by business_id,network_id 


SELECT
		business_id,
		network_id,
		CAST(SUM(subscribers) AS FLOAT)  as [managed_subscribers]
INTO #MANAGED_BY
FROM (
SELECT
		n.managed_business_id as business_id,
		n.zone_id,
		n.network_id,
		CAST(MAX(n.subscribers) AS FLOAT)  as subscribers
FROM
	#media_month1_universe n 
WHERE 
	n.owned_business_id != n.managed_business_id
group by n.managed_business_id ,n.zone_id, n.network_id, n.dma_id ) AS E
group by business_id,network_id 

-- Get the result
select distinct 
	ms.business_id as MvpdId, 
	ms.network_id as NetworkId,
	ms.name as Name, 
	network_name as NetworkName,
	month2_network_id as Month2NetworkId,
	month2_network_name as Month2NetworkName,
	ISNULL(own.[owned_subscribers],0) as [TotalOwnedSubscribers],
	ISNULL(mng.[managed_subscribers],0) as [TotalManagedSubscribers],
	ISNULL(ms.month1_subscribers,0) as [TotalMonth1Subscribers], 
	ISNULL(ms.[month2_subscribers],0) as [TotalMonth2Subscribers],
	@media_month1_id as [MediaMonthId],
	cucn.id as [ReviewId],
	cucn.reviewed_user_id as [ReviewedByUserId],
	emp.username as [ReviewedBy],
	cucn.reviewed_time as [ReviewedTime]
from #MVPD_LIST  ms
LEFT JOIN #OWNED_BY own (NOLOCK) on own.business_id = ms.business_id AND own.network_id = ms.network_id
LEFT JOIN #MANAGED_BY mng (NOLOCK) on mng.business_id = ms.business_id AND mng.network_id = ms.network_id
LEFT JOIN coverage_universe_comparison_mso cucn (NOLOCK) on cucn.media_month_id = @media_month1_id AND cucn.mso_id = ms.business_id
LEFT JOIN employees emp (NOLOCK) on emp.id = cucn.reviewed_user_id

--zone level

CREATE TABLE #coverage_universe_zone_month1 
(
 media_month_id int, 
 network_id int, 
 traffic_network_id int,
 network_code varchar(15), 
 owned_business_id int, 
 managed_business_id int, 
 managed_business_name  varchar(63), 
 dma_id int,
 dma_name varchar(63),
 zone_group_id int, 
 zone_group__code varchar(15),
 zone_group_name varchar(63),
 zone_id int, 
 zone_code varchar(15),
 zone_name varchar(63),
 zone_type varchar(63),
 zone_primary bit, 
 zone_network_primary bit,
 subscribers float
)
INSERT #coverage_universe_zone_month1  Exec usp_CUS_GetCoverageUniverseZoneData @media_month1_id;

select  
	network_id as ZoneNetworkId,
	isnull(traffic_network_id,network_id) as TrafficNetworkId,
	network_code as ZoneNetworkCode,
	owned_business_id as OwnedBusinessId,
	managed_business_id as MvpdId,
	managed_business_name as MvpdName,
	dma_id as MarketId,
	dma_name as MarketName,
	zone_group_id as ZoneGroupId,
	zone_group__code as ZoneGroupSysCode,
	zone_group_name as ZoneGroupName,
	zone_id as ZoneId,
	zone_code as SysCode,
	zone_name as ZoneName,
	zone_type as ZoneType,
	zone_primary as IsZonePrimary,
	zone_network_primary as IsZoneNetworkPrimary,
	subscribers as TotalSubscribers
from #coverage_universe_zone_month1

CREATE TABLE #coverage_universe_zone_month2 
(
 media_month_id int, 
 network_id int, 
 traffic_network_id int,
 network_code varchar(15), 
 owned_business_id int, 
 managed_business_id int, 
 managed_business_name  varchar(63), 
 dma_id int,
 dma_name varchar(63),
 zone_group_id int, 
 zone_group__code varchar(15),
 zone_group_name varchar(63),
 zone_id int, 
 zone_code varchar(15),
 zone_name varchar(63),
 zone_type varchar(63),
 zone_primary bit, 
 zone_network_primary bit,
 subscribers float
)
INSERT #coverage_universe_zone_month2  Exec usp_CUS_GetCoverageUniverseZoneData @media_month2_id;

select  
	network_id as ZoneNetworkId,
	isnull(traffic_network_id,network_id) as TrafficNetworkId,
	network_code as ZoneNetworkCode,
	owned_business_id as OwnedBusinessId,
	managed_business_id as MvpdId,
	managed_business_name as MvpdName,
	dma_id as MarketId,
	dma_name as MarketName,
	zone_group_id as ZoneGroupId,
	zone_group__code as ZoneGroupSysCode,
	zone_group_name as ZoneGroupName,
	zone_id as ZoneId,
	zone_code as SysCode,
	zone_name as ZoneName,
	zone_type as ZoneType,
	zone_primary as IsZonePrimary,
	zone_network_primary as IsZoneNetworkPrimary,
	subscribers as TotalSubscribers
from #coverage_universe_zone_month2


CREATE TABLE #coverage_universe_warnings (ZoneId int, ErrorType int);
-- ErrorType = DuplicateMarkets = 1, DuplicateManagedBusiness = 2, DuplicateOwnedBusiness =3

INSERT INTO #coverage_universe_warnings 
SELECT distinct	zone_id , 1
FROM
	#media_month1_universe n 
GROUP BY media_month_id,network_id,network_code,owned_business_id,managed_business_id, zone_id
HAVING COUNT(1) > 1

INSERT INTO #coverage_universe_warnings 
SELECT distinct	zone_id , 2
FROM
	#media_month1_universe n 
GROUP BY media_month_id,network_id,network_code,owned_business_id, dma_id, zone_id
HAVING COUNT(1) > 1

INSERT INTO #coverage_universe_warnings 
SELECT distinct	zone_id , 3
FROM
	#media_month1_universe n 
GROUP BY media_month_id,network_id,network_code, managed_business_id, dma_id, zone_id
HAVING COUNT(1) > 1

select * from #coverage_universe_warnings

END