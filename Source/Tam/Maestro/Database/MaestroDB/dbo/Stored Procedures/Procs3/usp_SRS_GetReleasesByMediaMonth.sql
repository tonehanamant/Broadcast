
CREATE PROCEDURE [dbo].[usp_SRS_GetReleasesByMediaMonth]
(
      @media_month varchar(4)
)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
SET NOCOUNT ON;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
			select distinct
			rel.id id, rel.name + ', ' + rel.description + ', Released ' +  cast(rel.release_date as varchar)
			+ ', Rls Id: ' + cast(rel.id as varchar) Label
			from
				media_months mm 
				join traffic_orders tr_ord  on
					mm.start_date <= tr_ord.end_date
					and
					tr_ord.start_date <= mm.end_date
				join traffic_details tr_dtl  on
					tr_dtl.id = tr_ord.traffic_detail_id
				join traffic tr  on
					tr.id = tr_dtl.traffic_id
				join releases rel  on
					rel.id = tr.release_id
					and
					mm.start_date <= rel.release_date
					and
					rel.release_date <= mm.end_date
				join statuses rel_stat  on
					rel_stat.id = rel.status_id
					and
					'releases' = rel_stat.status_set
					and
					'released' = rel_stat.name
			where
				@media_month = mm.media_month
				and
				1 = tr_ord.on_financial_reports
				and
				1 = tr_ord.active
END
