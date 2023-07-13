select * from spot_exceptions_out_of_spec_decisions

select * from spot_exceptions_out_of_specs

alter table spot_exceptions_out_of_spec_decisions drop column Comments


select * from spot_exceptions_recommended_plan_decision
select * from spot_exceptions_recommended_plans
select * from spot_exceptions_recommended_plan_details
INSERT INTO [dbo].[spot_exceptions_recommended_plan_decision]
([spot_exceptions_recommended_plan_detail_id]
,[username]
,[created_at]
,[synced_by]
,[synced_at])
VALUES
(
11
,'MockData'
,'2022-01-25 19:19:21.777'
,null,'2021-10-10T00:00:00.000')
GO

INSERT INTO [dbo].[spot_exceptions_recommended_plans]
           ([estimate_id]
           ,[isci_name]
           ,[recommended_plan_id]
           ,[program_name]
           ,[program_air_time]
           ,[station_legacy_call_letters]
           ,[cost]
           ,[impressions]
           ,[spot_length_id]
           ,[audience_id]
           ,[product]
           ,[flight_start_date]
           ,[flight_end_date]
           ,[daypart_id]
           ,[ingested_by]
           ,[ingested_at]
           ,[advertiser_name]
           ,[unique_id_external]
           ,[execution_id_external])
     VALUES
           (12345
           ,'3HRTSHF6'
           ,245
           ,'Q13 news at 10'
           ,'2021-10-04 00:00:00.000'
           ,'KOB'
           ,675.00
           ,765
           ,12
           ,431
           ,'Pizza Hut'
           ,'2019-08-01 00:00:00.000'
           ,'2019-09-01 00:00:00.000'
           ,70615
           ,'Mock Data'
           ,'2022-04-20 02:48:12.000'
           ,'Abbott Labs (Original)'
           ,2
           ,'d56a1bad-bf26-4847-9988-1135b734088f')
GO