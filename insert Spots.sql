select * from spot_exceptions_out_of_specs
select * from spot_exceptions_out_of_spec_decisions

SET IDENTITY_INSERT [dbo].[spot_exceptions_out_of_specs] ON
GO
INSERT [dbo].[spot_exceptions_out_of_specs] ([id], [reason_code_message], [estimate_id], [isci_name], 
[recommended_plan_id], [program_name], [station_legacy_call_letters], [spot_length_id], [audience_id], [product], 
[flight_start_date], [flight_end_date], [daypart_id], [program_daypart_id], [program_flight_start_date],
 [program_flight_end_date], [program_network], [program_audience_id], [program_air_time], [ingested_by], [ingested_at],
  [advertiser_name], [reason_code_id], [unique_id_external], [execution_id_external], [impressions],[market_code],[market_rank]) 
  VALUES (49, N'', 191756, N'AB82TXT2H', 216, N'Q13 news at 10', N'KOB', 12, 431, N'Pizza Hut', 
  CAST(N'2020-06-02T00:00:00.000' AS DateTime), CAST(N'2020-07-02T00:00:00.000' AS DateTime), NULL, 
  70641, CAST(N'2020-06-02T00:00:00.000' AS DateTime), CAST(N'2020-07-02T00:00:00.000' AS DateTime), N'',
   431, CAST(N'2021-10-10T00:00:00.000' AS DateTime), N'Mock Data', CAST(N'2022-01-25T11:45:48.533' AS DateTime),
    N'Chattem', 2, 2, N'e4bc675b-ab8e-4c3d-93d9-4aec93498638', 10000,null,null)
GO
SET IDENTITY_INSERT [dbo].[spot_exceptions_out_of_specs] OFF
GO

delete spot_exceptions_out_of_spec_decisions where id=13


INSERT INTO [dbo].[spot_exceptions_out_of_spec_decisions]
([spot_exceptions_out_of_spec_id]
,[accepted_as_in_spec]
,[decision_notes]
,[username]
,[created_at]
,[synced_by]
,[synced_at])
VALUES
(
67
,1
,''
,'MockData'
,'2022-01-25 19:19:21.777'
,null,'2021-10-10T00:00:00.000')
GO

SET IDENTITY_INSERT [dbo].[spot_exceptions_out_of_specs] ON
GO
INSERT [dbo].[spot_exceptions_out_of_specs] ([id],[spot_unique_hash_external],[house_isci], [reason_code_message], [estimate_id], [isci_name], 
[recommended_plan_id], [program_name], [station_legacy_call_letters], [spot_length_id], [audience_id],  
 [daypart_id],  [program_network], [program_air_time], [ingested_by], [ingested_at],
   [reason_code_id],  [execution_id_external], [impressions],[market_code],[market_rank],[program_genre_id],[created_by],[created_at],[modified_by],[modified_at]) 
  VALUES (28,'ME9DQUwtMTA1ODgzNzk3NN==','612NM15082H', N'', 191757, N'AB82TXT2H', 223, N'Q13 news at 10', N'KOB', 12, 431,
  70642,'test', CAST(N'2021-10-10T00:00:00.000' AS DateTime), 'testUser',CAST(N'2020-07-02T00:00:00.000' AS DateTime), 
   10,  N'Mock Data',10000,50,100,33,'mock data',CAST(N'2020-07-02T00:00:00.000' AS DateTime),'mock data',CAST(N'2020-07-02T00:00:00.000' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[spot_exceptions_out_of_specs] OFF
GO


select * from spot_exceptions_out_of_specs