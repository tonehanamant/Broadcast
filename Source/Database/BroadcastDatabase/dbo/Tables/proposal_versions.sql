CREATE TABLE [dbo].[proposal_versions] (
    [id]                     INT             IDENTITY (1, 1) NOT NULL,
    [proposal_id]            INT             NOT NULL,
    [proposal_version]       SMALLINT        NOT NULL,
    [start_date]             DATETIME        NULL,
    [end_date]               DATETIME        NULL,
    [guaranteed_audience_id] INT             NOT NULL,
    [markets]                TINYINT         NULL,
    [created_by]             VARCHAR (63)    NOT NULL,
    [created_date]           DATETIME        NOT NULL,
    [modified_by]            VARCHAR (63)    NOT NULL,
    [modified_date]          DATETIME        NOT NULL,
    [target_budget]          DECIMAL (19, 4) NULL,
    [target_units]           INT             NULL,
    [target_impressions]     FLOAT (53)      NOT NULL,
    [notes]                  VARCHAR (4000)  NULL,
    [post_type]              TINYINT         NOT NULL,
    [equivalized]            BIT             NOT NULL,
    [blackout_markets]       TINYINT         NULL,
    [status]                 TINYINT         NOT NULL,
    [target_cpm]             DECIMAL (19, 4) NOT NULL,
    [margin]                 FLOAT (53)      NOT NULL,
    [cost_total]             DECIMAL (19, 4) NOT NULL,
    [impressions_total]      FLOAT (53)      NOT NULL,
    [market_coverage]        FLOAT (53)      NULL,
    [snapshot_date]          DATETIME        NULL,
    CONSTRAINT [PK_proposal_versions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_versions_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_versions_proposals]
    ON [dbo].[proposal_versions]([proposal_id] ASC);

