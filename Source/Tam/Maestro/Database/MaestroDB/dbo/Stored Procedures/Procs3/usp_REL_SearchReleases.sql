
CREATE PROCEDURE [dbo].[usp_REL_SearchReleases]
      @name as varchar(100),
        @start_date as datetime,
        @end_date datetime,
        @advertiser_ids as VarChar(5000),
		@traffic_ids as VarChar(5000),
                                @product as varchar(100),
                                @iscii as varchar(100),
        @traffic_id int
AS

declare @query as varchar(max);

set @query = '
      select distinct
            releases.id,
            releases.category_id,
            releases.status_id,
            releases.name,
            releases.description,
            releases.notes,
            releases.release_date,
            releases.confirm_by_date
      from releases (NOLOCK) 
      left join traffic (NOLOCK) on traffic.release_id = releases.id';

if(@advertiser_ids is not null OR @product is not null)
  begin
      set @query = @query + ' join traffic_proposals tp WITH (NOLOCK) on traffic.id = tp.traffic_id join proposals p WITH (NOLOCK) on p.id = tp.proposal_id'; 
  end;
  
  if(@product is not null)
  begin
                  set @query = @query + ' left join products prod WITH (NOLOCK) on prod.id = p.product_id '; 
  end;
  

if(@iscii is not null)
  begin
      set @query = @query + ' left join traffic_materials tm WITH (NOLOCK) on tm.traffic_id = traffic.id join reel_materials rm WITH (NOLOCK) on rm.id = tm.reel_material_id join materials m WITH (NOLOCK) on m.id = rm.material_id '; 
  end;
  

set @query = @query + ' where 1=1 ';

if(@traffic_id is not null)
begin
      set @query = @query + ' and (traffic.id = ' + cast(@traffic_id as varchar) + ' or traffic.original_traffic_id = ' + cast(@traffic_id as varchar) + ')'; 
end;
else
begin;
            
      if(@start_date is not null)
      begin
            set @query = @query + ' and releases.release_date >= ''' + convert(varchar, @start_date, 101) + ''''; 
      end;

      if(@end_date is not null)
      begin
            set @query =  @query + ' and releases.release_date <= ''' + convert(varchar, @end_date, 101) + ''''; 
      end;

      if(@name is not null)
      begin
            set @query = @query + ' and releases.name like ''' + @name + '%'''; 
      end;

      if(@advertiser_ids is not null)
      begin
            set @query = @query + ' and (traffic.id in ' + @traffic_ids+ ' OR p.advertiser_company_id in ' + @advertiser_ids + ')'; 
      end;
                  
                  if(@product is not null)
      begin
            set @query = @query + ' and (prod.name like ''%' + @product + '%'' OR traffic.description like ''%' + @product + '%'')'; 
      end;
                  
                  if(@iscii is not null)
      begin
            set @query = @query + ' and (m.code like ''%' + @iscii + '%'')'; 
      end;
end;


set @query = @query + ' order by releases.name';

--print @query;


exec (@query);
