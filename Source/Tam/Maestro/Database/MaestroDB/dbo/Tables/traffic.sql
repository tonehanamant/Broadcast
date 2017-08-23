CREATE TABLE [dbo].[traffic] (
    [id]                           INT             IDENTITY (1, 1) NOT NULL,
    [status_id]                    INT             NOT NULL,
    [release_id]                   INT             NULL,
    [audience_id]                  INT             NOT NULL,
    [original_traffic_id]          INT             NULL,
    [traffic_category_id]          INT             NULL,
    [ratings_daypart_id]           INT             NOT NULL,
    [revision]                     INT             NOT NULL,
    [name]                         VARCHAR (63)    NOT NULL,
    [display_name]                 VARCHAR (63)    NOT NULL,
    [description]                  VARCHAR (127)   NOT NULL,
    [comment]                      VARCHAR (255)   NOT NULL,
    [priority]                     INT             NOT NULL,
    [start_date]                   DATETIME        NOT NULL,
    [end_date]                     DATETIME        NOT NULL,
    [date_created]                 DATETIME        NOT NULL,
    [date_last_modified]           DATETIME        NOT NULL,
    [base_ratings_media_month_id]  INT             NOT NULL,
    [internal_note_id]             INT             NULL,
    [external_note_id]             INT             NULL,
    [adsrecovery_base]             BIT             CONSTRAINT [DF_traffic_adsrecovery_base] DEFAULT ((0)) NOT NULL,
    [percent_discount]             FLOAT (53)      CONSTRAINT [DF_traffic_percent_discount] DEFAULT ((0)) NOT NULL,
    [rate_card_type_id]            INT             CONSTRAINT [DF_traffic_rate_card_type_id] DEFAULT ((1)) NOT NULL,
    [sort_order]                   INT             NULL,
    [product_description_id]       INT             NULL,
    [base_universe_media_month_id] INT             NOT NULL,
    [base_index_media_month_id]    INT             NULL,
    [make_good]                    BIT             DEFAULT (CONVERT([bit],(0),0)) NOT NULL,
    [net_factors]                  DECIMAL (19, 8) NULL,
    [plan_type]                    TINYINT         CONSTRAINT [DF_traffic_plan_type] DEFAULT ((0)) NOT NULL,
    [notes]                        TEXT            NULL,
    CONSTRAINT [PK_traffic] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_traffic_dayparts] FOREIGN KEY ([ratings_daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_external_notes] FOREIGN KEY ([external_note_id]) REFERENCES [dbo].[notes] ([id]),
    CONSTRAINT [FK_traffic_internal_note] FOREIGN KEY ([internal_note_id]) REFERENCES [dbo].[notes] ([id]),
    CONSTRAINT [FK_traffic_media_month] FOREIGN KEY ([base_ratings_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_traffic_media_months] FOREIGN KEY ([base_universe_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_traffic_media_months1] FOREIGN KEY ([base_index_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_traffic_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id]),
    CONSTRAINT [FK_traffic_releases] FOREIGN KEY ([release_id]) REFERENCES [dbo].[releases] ([id]),
    CONSTRAINT [FK_traffic_statuses] FOREIGN KEY ([status_id]) REFERENCES [dbo].[statuses] ([id]),
    CONSTRAINT [FK_traffic_traffic] FOREIGN KEY ([original_traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [FK_traffic_traffic_categories] FOREIGN KEY ([traffic_category_id]) REFERENCES [dbo].[traffic_categories] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_traffic_flight]
    ON [dbo].[traffic]([start_date] ASC, [end_date] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_original_traffic_id]
    ON [dbo].[traffic]([original_traffic_id] ASC) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_audience_id]
    ON [dbo].[traffic]([audience_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'percent_discount';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'percent_discount';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'sort_order';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'sort_order';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'product_description_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'product_description_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_index_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_index_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'make_good';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'make_good';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'release_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'release_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'original_traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'original_traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'traffic_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'traffic_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'ratings_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'ratings_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'revision';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'revision';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'display_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'description';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'description';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'comment';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'priority';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'priority';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'internal_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'internal_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'external_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'external_note_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'adsrecovery_base';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic', @level2type = N'COLUMN', @level2name = N'adsrecovery_base';

