
 --=============================================
 --Author:		David Sisson
 --Create date: 03/16/2009
 --Description:	Returns zone_histories as of specified date.
 --=============================================
CREATE FUNCTION [dbo].[udf_ReleasedTraffic]
(	
	@codeMonth varchar(5),
	@ReleasedOnly bit
)
RETURNS TABLE
AS
RETURN 
(
	select
		tr.id traffic_id, 
		tr.status_id, 
		tr.release_id, 
		tr.audience_id, 
		tr.original_traffic_id, 
		tr.traffic_category_id, 
		tr.ratings_daypart_id, 
		tr.revision, 
		tr.name, 
		tr.display_name, 
		tr.description, 
		tr.comment, 
		tr.priority, 
		tr.start_date, 
		tr.end_date, 
		tr.date_created, 
		tr.date_last_modified, 
		tr.base_ratings_media_month_id, 
		tr.internal_note_id, 
		tr.external_note_id, 
		tr.adsrecovery_base, 
		tr.percent_discount, 
		tr.rate_card_type_id, 
		tr.sort_order, 
		tr.product_description_id, 
		tr.base_universe_media_month_id, 
		tr.base_index_media_month_id
	from
		media_months mm (nolock)
		join traffic tr (nolock) on
			mm.start_date <= tr.end_date
			and
			tr.start_date <= mm.end_date
			--tr.id = tr_dtl.traffic_id
		join releases rel (nolock) on
			rel.id = tr.release_id
		join statuses tr_stat (nolock) on
			tr_stat.id = tr.status_id
			and
			'traffic' = tr_stat.status_set
		join statuses rel_stat (nolock) on
			rel_stat.id = rel.status_id
			and
			'releases' = rel_stat.status_set
	where
		@codeMonth = mm.media_month
		and
		'Release Order' = tr_stat.name
		and
		(@ReleasedOnly = 0 or 'released' = rel_stat.name)
);
