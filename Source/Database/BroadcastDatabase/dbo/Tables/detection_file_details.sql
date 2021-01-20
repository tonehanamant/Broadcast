CREATE TABLE [dbo].[detection_file_details] (
    [id]                           INT           IDENTITY (1, 1) NOT NULL,
    [detection_file_id]            INT           NOT NULL,
    [rank]                         INT           NOT NULL,
    [market]                       VARCHAR (63)  NOT NULL,
    [station]                      VARCHAR (15)  NOT NULL,
    [affiliate]                    VARCHAR (15)  NOT NULL,
    [date_aired]                   DATETIME      NOT NULL,
    [time_aired]                   INT           NOT NULL,
    [program_name]                 VARCHAR (255) NOT NULL,
    [spot_length]                  INT           NOT NULL,
    [spot_length_id]               INT           NULL,
    [isci]                         VARCHAR (63)  NOT NULL,
    [estimate_id]                  INT           NOT NULL,
    [schedule_detail_week_id]      INT           NULL,
    [match_station]                BIT           NOT NULL,
    [match_program]                BIT           NOT NULL,
    [match_airtime]                BIT           NOT NULL,
    [match_isci]                   BIT           NOT NULL,
    [status]                       INT           NOT NULL,
    [nsi_date]                     DATETIME      NOT NULL,
    [nti_date]                     DATETIME      NOT NULL,
    [has_lead_in_schedule_matches] BIT           NOT NULL,
    [linked_to_block]              BIT           NOT NULL,
    [linked_to_leadin]             BIT           NOT NULL,
    [match_spot_length]            BIT           NOT NULL,
    [advertiser]                   VARCHAR (63)  NULL,
    CONSTRAINT [PK_detection_file_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_detection_file_details_schedule_detail_weeks] FOREIGN KEY ([schedule_detail_week_id]) REFERENCES [dbo].[schedule_detail_weeks] ([id]),
    CONSTRAINT [FK_detection_file_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_detection_file_detection_file_details] FOREIGN KEY ([detection_file_id]) REFERENCES [dbo].[detection_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_detection_file_detection_file_details]
    ON [dbo].[detection_file_details]([detection_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_detection_file_details_spot_lengths]
    ON [dbo].[detection_file_details]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_detection_file_details_schedule_detail_weeks]
    ON [dbo].[detection_file_details]([schedule_detail_week_id] ASC);

