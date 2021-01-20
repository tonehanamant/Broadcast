CREATE TABLE [dbo].[spot_tracker_file_details] (
    [id]                   INT           IDENTITY (1, 1) NOT NULL,
    [spot_tracker_file_id] INT           NOT NULL,
    [client]               VARCHAR (15)  NULL,
    [client_name]          VARCHAR (63)  NULL,
    [advertiser]           VARCHAR (63)  NULL,
    [release_name]         VARCHAR (63)  NULL,
    [isci]                 VARCHAR (63)  NOT NULL,
    [spot_length_id]       INT           NULL,
    [spot_length]          INT           NOT NULL,
    [country]              VARCHAR (63)  NULL,
    [rank]                 INT           NULL,
    [market]               VARCHAR (63)  NULL,
    [market_code]          INT           NULL,
    [station]              VARCHAR (15)  NOT NULL,
    [station_name]         VARCHAR (64)  NULL,
    [affiliate]            VARCHAR (15)  NULL,
    [date_aired]           DATETIME      NOT NULL,
    [day_of_week]          VARCHAR (2)   NULL,
    [daypart]              VARCHAR (10)  NULL,
    [time_aired]           INT           NOT NULL,
    [program_name]         VARCHAR (255) NULL,
    [encode_date]          DATETIME      NULL,
    [encode_time]          INT           NULL,
    [rel_type]             VARCHAR (15)  NULL,
    [estimate_id]          INT           NOT NULL,
    [identifier_2]         INT           NULL,
    [identifier_3]         INT           NULL,
    [sid]                  INT           NULL,
    [discid]               INT           NULL,
    CONSTRAINT [PK_spot_tracker_file_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_spot_tracker_file_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_spot_tracker_file_spot_tracker_file_details] FOREIGN KEY ([spot_tracker_file_id]) REFERENCES [dbo].[spot_tracker_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_spot_tracker_file_spot_tracker_file_details]
    ON [dbo].[spot_tracker_file_details]([spot_tracker_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_spot_tracker_file_details_spot_lengths]
    ON [dbo].[spot_tracker_file_details]([spot_length_id] ASC);

