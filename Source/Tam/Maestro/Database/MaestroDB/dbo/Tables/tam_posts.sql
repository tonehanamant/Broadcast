CREATE TABLE [dbo].[tam_posts] (
    [id]                               INT           IDENTITY (1, 1) NOT NULL,
    [rating_source_id]                 TINYINT       NOT NULL,
    [rate_card_type_id]                INT           NOT NULL,
    [post_type_code]                   TINYINT       NOT NULL,
    [title]                            VARCHAR (127) NOT NULL,
    [is_deleted]                       BIT           NOT NULL,
    [is_equivalized]                   BIT           NOT NULL,
    [network_delivery_cap_percentage]  FLOAT (53)    NULL,
    [post_setup_advertiser]            VARCHAR (511) NOT NULL,
    [post_setup_agency]                VARCHAR (511) NOT NULL,
    [post_setup_daypart]               VARCHAR (511) NOT NULL,
    [post_setup_product]               VARCHAR (511) NOT NULL,
    [override_advertiser]              BIT           NOT NULL,
    [override_agency]                  BIT           NOT NULL,
    [override_daypart]                 BIT           NOT NULL,
    [override_product]                 BIT           NOT NULL,
    [multiple_product_post]            BIT           CONSTRAINT [DF_tam_posts_multiple_product_post] DEFAULT ((0)) NOT NULL,
    [strict_start_time]                BIT           CONSTRAINT [DF_tam_posts_strict_start_time_1] DEFAULT ((0)) NOT NULL,
    [strict_end_time]                  BIT           CONSTRAINT [DF_tam_posts_strict_end_time_1] DEFAULT ((0)) NOT NULL,
    [created_by_employee_id]           INT           NOT NULL,
    [modified_by_employee_id]          INT           NULL,
    [deleted_by_employee_id]           INT           NULL,
    [locked]                           BIT           NOT NULL,
    [locked_by_employee_id]            INT           NULL,
    [number_of_zones_delivering]       INT           CONSTRAINT [DF_tam_posts_number_of_zones_delivering] DEFAULT ((0)) NOT NULL,
    [date_created]                     DATETIME      NOT NULL,
    [date_last_modified]               DATETIME      NULL,
    [date_deleted]                     DATETIME      NULL,
    [report_weekly_pacing]             BIT           CONSTRAINT [DF_tam_posts_report_weekly_pacing] DEFAULT ((0)) NOT NULL,
    [exclude_from_year_to_date_report] BIT           DEFAULT ((0)) NOT NULL,
    [full_fast_tracks]                 BIT           DEFAULT ((0)) NOT NULL,
    [is_msa]                           BIT           CONSTRAINT [DF_tam_posts_is_msa] DEFAULT ((0)) NOT NULL,
    [campaign_id]                      INT           NULL,
    [produce_monthy_posts]             BIT           NOT NULL,
    [produce_quarterly_posts]          BIT           NOT NULL,
    [produce_full_posts]               BIT           NOT NULL,
    CONSTRAINT [PK_tam_posts] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_posts_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id]),
    CONSTRAINT [FK_tam_posts_employees] FOREIGN KEY ([created_by_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_tam_posts_employees1] FOREIGN KEY ([modified_by_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_tam_posts_employees2] FOREIGN KEY ([locked_by_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_tam_posts_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id]),
    CONSTRAINT [FK_tam_posts_rating_sources] FOREIGN KEY ([rating_source_id]) REFERENCES [dbo].[rating_sources] ([id])
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_type_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_type_code';


GO



GO



GO



GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Determines if this post is deleted or active.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'is_deleted';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'is_deleted';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'is_equivalized';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'is_equivalized';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'network_delivery_cap_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'network_delivery_cap_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_agency';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_agency';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_product';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'post_setup_product';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_advertiser';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_agency';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_agency';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_product';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'override_product';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'multiple_product_post';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'multiple_product_post';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'strict_start_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'strict_start_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'strict_end_time';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'strict_end_time';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'created_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'created_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'modified_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'modified_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'deleted_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'deleted_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'locked';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'locked';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'locked_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'locked_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'number_of_zones_delivering';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'number_of_zones_delivering';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_deleted';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'date_deleted';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'report_weekly_pacing';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'report_weekly_pacing';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'exclude_from_year_to_date_report';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_posts', @level2type = N'COLUMN', @level2name = N'exclude_from_year_to_date_report';

