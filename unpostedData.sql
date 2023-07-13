select *  from spot_exceptions_unposted_no_plan

select * from spot_exceptions_unposted_no_reel_roster

select * from spot_exceptions_out_of_specs

SET IDENTITY_INSERT [dbo].[spot_exceptions_unposted_no_plan] ON
GO
insert into spot_exceptions_unposted_no_plan(id,house_isci,client_isci,client_spot_length_id,count,program_air_time,estimate_id,ingested_by,ingested_at,created_by,created_at,modified_by,modified_at) 
values(4,'XB82TXT8M','AB82VR592',14,2,CAST(N'2021-10-10T00:00:00.000' AS DateTime),191760,'Mock Data',CAST(N'2021-10-10T00:00:00.000' AS DateTime),'Mock',CAST(N'2021-10-10T00:00:00.000' AS DateTime),'Mock',CAST(N'2021-10-10T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[spot_exceptions_unposted_no_plan] OFF
GO

SET IDENTITY_INSERT [dbo].spot_exceptions_unposted_no_reel_roster ON
GO
insert into spot_exceptions_unposted_no_reel_roster(id,house_isci,count,program_air_time,estimate_id,ingested_by,ingested_at,created_by,created_at,modified_by,modified_at) 
values(1,'XB82TXT8M',2,CAST(N'2021-10-10T00:00:00.000' AS DateTime),191760,'Mock Data',CAST(N'2021-10-10T00:00:00.000' AS DateTime),'Mock',CAST(N'2021-10-10T00:00:00.000' AS DateTime),'Mock',CAST(N'2021-10-10T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].spot_exceptions_unposted_no_reel_roster OFF
GO