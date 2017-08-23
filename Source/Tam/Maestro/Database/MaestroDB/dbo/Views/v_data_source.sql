create view [dbo].[v_data_source] as
	select 'maestro' as maestro_name, getdate() as maestro_dt,'postlog_staging' as postlog_name, getdate() as postlog_dt,
	'cmw_analysis' as cmw_analysis_name, getdate() as cmw_analysis_dt