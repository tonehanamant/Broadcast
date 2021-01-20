CREATE TABLE [dbo].[pricing_guide_distributions] (
    [id]                               INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id]       INT             NOT NULL,
    [adjustment_margin]                FLOAT (53)      NULL,
    [adjustment_impression_loss]       FLOAT (53)      NULL,
    [adjustment_inflation]             FLOAT (53)      NULL,
    [goal_impression]                  FLOAT (53)      NULL,
    [goal_budget]                      DECIMAL (19, 4) NULL,
    [open_market_cpm_min]              DECIMAL (19, 4) NULL,
    [open_market_cpm_max]              DECIMAL (19, 4) NULL,
    [open_market_unit_cap_per_station] INT             NULL,
    [open_market_cpm_target]           TINYINT         NULL,
    [total_open_market_cpm]            DECIMAL (19, 4) NOT NULL,
    [total_open_market_cost]           DECIMAL (19, 4) NOT NULL,
    [total_open_market_impressions]    FLOAT (53)      NOT NULL,
    [total_open_market_coverage]       FLOAT (53)      NOT NULL,
    [total_proprietary_cpm]            DECIMAL (19, 4) NOT NULL,
    [total_proprietary_cost]           DECIMAL (19, 4) NOT NULL,
    [total_proprietary_impressions]    FLOAT (53)      NOT NULL,
    [created_date]                     DATETIME        NOT NULL,
    [created_by]                       VARCHAR (63)    NOT NULL,
    [market_coverage_file_id]          INT             NOT NULL,
    CONSTRAINT [PK_pricing_guide_distributions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_pricing_guide_distributions_market_coverage_files] FOREIGN KEY ([market_coverage_file_id]) REFERENCES [dbo].[market_coverage_files] ([id]),
    CONSTRAINT [FK_pricing_guide_distributions_proposal_versions] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distributions_market_coverage_files]
    ON [dbo].[pricing_guide_distributions]([market_coverage_file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_pricing_guide_distributions_proposal_versions]
    ON [dbo].[pricing_guide_distributions]([proposal_version_detail_id] ASC);

