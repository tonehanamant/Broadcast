

-- EXEC [usp_ARS_GetMarketsAndStationsForTraffic] null, null, '100', null, null, '01/23/2012'
CREATE PROCEDURE [dbo].[usp_BS_GetMarketsAndStationsForTraffic]
(
                @market_include_list varchar(max),
                @market_exclude_list varchar(max),
                @market_rank_include_list varchar(max),
                @market_rank_exclude_list varchar(max),
                @source_include_list varchar(max),
                @effective_date datetime

)
AS
BEGIN 
                
                -- We need to use the latest sweeps month
                declare @media_month_id int;

                -- GET THE LATEST NOVEMBER MONTH WE HAVE LOADED IN THE SYSTEM!
                select @media_month_id = max(distinct mh.media_month_id) FROM external_rating.nsi.market_headers mh WITH (NOLOCK)
                                                join media_months mm WITH (NOLOCK) on mm.id = mh.media_month_id
                where mm.month = 11

                declare @query varchar(max);
                set @query = '';
                set @query = @query + 'SELECT 
                                distinct pmc.market_rank, pmc.market_code, max(pmc.station_code), pmc.broadcast_channel_number, pmc.call_letters
                                , tz.name, tz.id, mh.geography_name, sz.system_id, z.zone_id, pmc.affiliation
                from 
                                external_rating.dbo.program_monthly_schedule pmc WITH (NOLOCK)
                                join zone_maps zm1 with (NOLOCK) on zm1.map_value = pmc.station_code and zm1.map_set = ''Brdcst Callltr''   
                                join uvw_zone_universe z WITH (NOLOCK) on z.zone_id = zm1.zone_id and (z.start_date<=''' + convert(varchar(20),@effective_date, 101) + ''' AND (z.end_date>=''' + convert(varchar(20),@effective_date, 101) + ''' OR z.end_date IS NULL))
                                join uvw_systemzone_universe sz with (NOLOCK) on sz.zone_id = z.zone_id and (sz.start_date<=''' + convert(varchar(20),@effective_date, 101) + ''' AND (sz.end_date>=''' + convert(varchar(20),@effective_date, 101) + ''' OR sz.end_date IS NULL))
                                join zone_maps zm with (NOLOCK) on zm.zone_id = z.zone_id and zm.map_set = ''Brdcst Timezone''
                                join external_rating.nsi.market_headers mh with (NOLOCK) on mh.media_month_id = pmc.media_month_id and mh.market_code = pmc.market_code 
                                join time_zones tz on tz.id = cast(zm.map_value as int) '; 
                
                set @query = @query + ' WHERE
                pmc.media_month_id = ' + cast(@media_month_id as varchar);
                
                IF(@market_rank_include_list IS NOT NULL)
                BEGIN
                                set @query = @query + ' AND pmc.market_rank <= ' + @market_rank_include_list;
                END
                
                IF(@market_rank_exclude_list IS NOT NULL)
                BEGIN
                                set @query = @query + ' AND pmc.market_rank >= ' + @market_rank_exclude_list;
                END
                                
                IF @market_include_list IS NOT NULL
                BEGIN
                                set @query = @query + ' AND pmc.market_code in (' + @market_include_list + ')';
                END
                IF @market_exclude_list IS NOT NULL
                BEGIN
                                set @query = @query + ' AND pmc.market_code not in (' + @market_exclude_list + ')';                
                END
                IF @source_include_list IS NOT NULL
                BEGIN
                                set @query = @query + ' AND pmc.program_source in (' + @source_include_list + ')';    
                END

                set @query = @query + ' group by pmc.market_rank, pmc.market_code, pmc.broadcast_channel_number, pmc.call_letters
                                , tz.name, tz.id, mh.geography_name, sz.system_id, z.zone_id, pmc.affiliation ';
                set @query = @query + ' order by pmc.market_rank, pmc.call_letters;';

print 'Q;'+ @query;
exec (@query);
                
END

