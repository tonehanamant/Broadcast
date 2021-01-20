CREATE TABLE [dbo].[postlog_client_scrubs] (
    [id]                                      INT            IDENTITY (1, 1) NOT NULL,
    [postlog_file_detail_id]                  BIGINT         NOT NULL,
    [proposal_version_detail_quarter_week_id] INT            NOT NULL,
    [lead_in]                                 BIT            NOT NULL,
    [effective_program_name]                  VARCHAR (255)  NULL,
    [effective_genre]                         VARCHAR (255)  NULL,
    [effective_show_type]                     VARCHAR (255)  NULL,
    [effective_isci]                          VARCHAR (63)   NULL,
    [effective_client_isci]                   VARCHAR (63)   NULL,
    [match_program]                           BIT            NOT NULL,
    [match_genre]                             BIT            NOT NULL,
    [match_market]                            BIT            NOT NULL,
    [match_time]                              BIT            NOT NULL,
    [match_station]                           BIT            NOT NULL,
    [match_isci_days]                         BIT            NOT NULL,
    [match_date]                              BIT            NULL,
    [match_show_type]                         BIT            NOT NULL,
    [match_isci]                              BIT            NOT NULL,
    [comment]                                 VARCHAR (1023) NULL,
    [modified_by]                             VARCHAR (255)  NOT NULL,
    [modified_date]                           DATETIME       NULL,
    [status]                                  INT            NOT NULL,
    [status_override]                         BIT            NOT NULL,
    CONSTRAINT [PK_postlog_client_scrubs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_postlog_file_details_postlog_client_scrubs] FOREIGN KEY ([postlog_file_detail_id]) REFERENCES [dbo].[postlog_file_details] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_version_detail_quarter_weeks_postlog_client_scrubs] FOREIGN KEY ([proposal_version_detail_quarter_week_id]) REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_quarter_weeks_postlog_client_scrubs]
    ON [dbo].[postlog_client_scrubs]([proposal_version_detail_quarter_week_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_postlog_file_details_postlog_client_scrubs]
    ON [dbo].[postlog_client_scrubs]([postlog_file_detail_id] ASC);

