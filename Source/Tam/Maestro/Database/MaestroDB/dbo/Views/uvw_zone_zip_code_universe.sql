CREATE VIEW [dbo].[uvw_zone_zip_code_universe]
AS
	SELECT     zzc.zone_id,zzc.zip_code,zzc.effective_date AS start_date, NULL AS end_date
	FROM       dbo.zone_zip_codes zzc (NOLOCK)
	UNION ALL
	SELECT     zzc.zone_id,zzc.zip_code, start_date, end_date
	FROM       dbo.zone_zip_code_histories zzc (NOLOCK)