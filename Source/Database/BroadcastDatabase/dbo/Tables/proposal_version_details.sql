CREATE TABLE [dbo].[proposal_version_details] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_id]           INT             NOT NULL,
    [spot_length_id]                INT             NOT NULL,
    [daypart_id]                    INT             NOT NULL,
    [daypart_code]                  VARCHAR (10)    NOT NULL,
    [start_date]                    DATETIME        NOT NULL,
    [end_date]                      DATETIME        NOT NULL,
    [units_total]                   INT             NULL,
    [impressions_total]             FLOAT (53)      NOT NULL,
    [cost_total]                    DECIMAL (19, 4) NULL,
    [adu]                           BIT             NOT NULL,
    [single_projection_book_id]     INT             NULL,
    [hut_projection_book_id]        INT             NULL,
    [share_projection_book_id]      INT             NULL,
    [projection_playback_type]      TINYINT         NOT NULL,
    [open_market_impressions_total] FLOAT (53)      NOT NULL,
    [open_market_cost_total]        DECIMAL (19, 4) NOT NULL,
    [proprietary_impressions_total] FLOAT (53)      NOT NULL,
    [proprietary_cost_total]        DECIMAL (19, 4) NOT NULL,
    [sequence]                      INT             NULL,
    [posting_book_id]               INT             NULL,
    [posting_playback_type]         TINYINT         NULL,
    [nti_conversion_factor]         FLOAT (53)      NULL,
    CONSTRAINT [PK_proposal_version_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_proposal_version_details_hut_media_months] FOREIGN KEY ([hut_projection_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_version_details_posting_media_months] FOREIGN KEY ([posting_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_version_details_proposal_versions] FOREIGN KEY ([proposal_version_id]) REFERENCES [dbo].[proposal_versions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_version_details_share_media_months] FOREIGN KEY ([share_projection_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_version_details_single_media_months] FOREIGN KEY ([single_projection_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_version_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_spot_lengths]
    ON [dbo].[proposal_version_details]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_proposal_versions]
    ON [dbo].[proposal_version_details]([proposal_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_single_media_months]
    ON [dbo].[proposal_version_details]([single_projection_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_share_media_months]
    ON [dbo].[proposal_version_details]([share_projection_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_posting_media_months]
    ON [dbo].[proposal_version_details]([posting_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_hut_media_months]
    ON [dbo].[proposal_version_details]([hut_projection_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_dayparts]
    ON [dbo].[proposal_version_details]([daypart_id] ASC);

