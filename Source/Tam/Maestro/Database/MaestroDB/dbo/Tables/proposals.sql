CREATE TABLE [dbo].[proposals] (
    [id]                                     INT            IDENTITY (1, 1) NOT NULL,
    [original_proposal_id]                   INT            NULL,
    [version_number]                         INT            CONSTRAINT [DF_proposal_versions_version_number] DEFAULT ((0)) NOT NULL,
    [name]                                   VARCHAR (127)  NOT NULL,
    [product_id]                             INT            NULL,
    [agency_company_id]                      INT            NOT NULL,
    [advertiser_company_id]                  INT            NULL,
    [default_spot_length_id]                 INT            NULL,
    [default_daypart_id]                     INT            NULL,
    [start_date]                             DATETIME       NOT NULL,
    [end_date]                               DATETIME       NOT NULL,
    [guarantee_type]                         TINYINT        NULL,
    [proposal_type_id]                       INT            NULL,
    [proposal_status_id]                     INT            NOT NULL,
    [rate_card_type_id]                      INT            NULL,
    [traffic_notes]                          TEXT           NOT NULL,
    [budget]                                 MONEY          CONSTRAINT [DF_proposals_budget] DEFAULT ((0)) NOT NULL,
    [is_equivalized]                         BIT            NOT NULL,
    [is_audience_deficiency_unit_schedule]   BIT            NULL,
    [include_on_availability_planner]        BIT            NOT NULL,
    [multiple_dayparts]                      BIT            NOT NULL,
    [multiple_flights]                       BIT            NOT NULL,
    [multiple_spot_lengths]                  BIT            NOT NULL,
    [base_ratings_media_month_id]            INT            NULL,
    [base_universe_media_month_id]           INT            NULL,
    [flight_text]                            VARCHAR (1027) NOT NULL,
    [optimization_goal]                      TINYINT        NULL,
    [optimization_target]                    VARCHAR (15)   CONSTRAINT [DF_proposals_optimization_target] DEFAULT ('0') NULL,
    [billing_terms_id]                       INT            CONSTRAINT [DF_proposals_billing_terms_id] DEFAULT ((3)) NOT NULL,
    [additional_information]                 TEXT           NULL,
    [salesperson_employee_id]                INT            NULL,
    [date_created]                           DATETIME       NULL,
    [date_last_modified]                     DATETIME       NULL,
    [network_rate_card_id]                   INT            NULL,
    [buyer_note]                             VARCHAR (2047) NULL,
    [print_title]                            VARCHAR (2047) NULL,
    [include_on_marriage_report]             BIT            CONSTRAINT [DF_proposals_include_on_marriage_report] DEFAULT ((0)) NOT NULL,
    [exclude_from_pending_report]            BIT            CONSTRAINT [DF_proposals_exclude_from_pending_report] DEFAULT ((0)) NOT NULL,
    [posting_media_month_id]                 INT            NULL,
    [billing_address_id]                     INT            NULL,
    [confirmation_notes]                     VARCHAR (2047) NULL,
    [primary_daypart_id]                     INT            NULL,
    [is_msa]                                 BIT            NULL,
    [number_of_materials]                    SMALLINT       CONSTRAINT [DF_proposals_number_of_materials] DEFAULT ((0)) NOT NULL,
    [total_spots]                            INT            CONSTRAINT [DF_proposals_number_of_spots] DEFAULT ((0)) NOT NULL,
    [total_gross_cost]                       MONEY          CONSTRAINT [DF_proposals_total_gross_cost] DEFAULT ((0)) NOT NULL,
    [is_upfront]                             BIT            NULL,
    [rating_source_id]                       TINYINT        CONSTRAINT [DF_proposals_rating_source_id] DEFAULT ((1)) NOT NULL,
    [local_pods_authorized]                  BIT            CONSTRAINT [DF_proposals_local_pods_authorized] DEFAULT ((0)) NOT NULL,
    [posting_notes]                          VARCHAR (2047) CONSTRAINT [DF_proposals_internal_notes] DEFAULT ('') NOT NULL,
    [media_plan_format_code]                 TINYINT        NULL,
    [finance_notes]                          VARCHAR (2047) DEFAULT ('') NOT NULL,
    [billing_notes]                          VARCHAR (2047) DEFAULT ('') NOT NULL,
    [media_plan_notes]                       VARCHAR (2047) DEFAULT ('') NOT NULL,
    [invoice_format_code]                    TINYINT        NULL,
    [is_invoice_weekly_gross_split_required] BIT            DEFAULT ((0)) NOT NULL,
    [are_iscis_required_on_invoice]          BIT            DEFAULT ((0)) NOT NULL,
    [billing_flow_chart_code]                TINYINT        DEFAULT ((0)) NOT NULL,
    [category_id]                            INT            NULL,
    [is_media_ocean_plan]                    BIT            CONSTRAINT [DF_proposals_is_media_ocean_plan] DEFAULT ((0)) NOT NULL,
    [is_cash_back]                           BIT            CONSTRAINT [DF_proposals_is_cash_back] DEFAULT ((0)) NOT NULL,
    [total_gross_cost_after_cash_back]       MONEY          CONSTRAINT [DF_proposals_total_gross_cost_after_cash_back] DEFAULT ((0)) NOT NULL,
    [campaign_id]                            INT            NULL,
    [sales_office_id]                        INT            NULL,
    [advertiser_industry_id]                 INT            NULL,
    [audience_deficiency_unit_for]           VARCHAR (31)   NULL,
    [is_overnight]                           BIT            NULL,
    [is_advanced_tv]                         BIT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_proposal_versions] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_proposals_advertiser_industries] FOREIGN KEY ([advertiser_industry_id]) REFERENCES [dbo].[advertiser_industries] ([id]),
    CONSTRAINT [FK_proposals_billing_terms] FOREIGN KEY ([billing_terms_id]) REFERENCES [dbo].[billing_terms] ([id]),
    CONSTRAINT [FK_proposals_campaigns] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id]),
    CONSTRAINT [FK_proposals_categories] FOREIGN KEY ([category_id]) REFERENCES [dbo].[categories] ([id]),
    CONSTRAINT [FK_proposals_companies] FOREIGN KEY ([advertiser_company_id]) REFERENCES [dbo].[companies] ([id]),
    CONSTRAINT [FK_proposals_dayparts] FOREIGN KEY ([default_daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_proposals_dayparts1] FOREIGN KEY ([primary_daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_proposals_employees] FOREIGN KEY ([salesperson_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposals_media_months] FOREIGN KEY ([base_ratings_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposals_media_months1] FOREIGN KEY ([base_universe_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposals_media_months2] FOREIGN KEY ([posting_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposals_network_rate_cards] FOREIGN KEY ([network_rate_card_id]) REFERENCES [dbo].[network_rate_cards] ([id]),
    CONSTRAINT [FK_proposals_products] FOREIGN KEY ([product_id]) REFERENCES [dbo].[products] ([id]),
    CONSTRAINT [FK_proposals_proposal_statuses] FOREIGN KEY ([proposal_status_id]) REFERENCES [dbo].[proposal_statuses] ([id]),
    CONSTRAINT [FK_proposals_proposal_types] FOREIGN KEY ([proposal_type_id]) REFERENCES [dbo].[proposal_types] ([id]),
    CONSTRAINT [FK_proposals_proposals1] FOREIGN KEY ([original_proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_proposals_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id]),
    CONSTRAINT [FK_proposals_rating_sources] FOREIGN KEY ([rating_source_id]) REFERENCES [dbo].[rating_sources] ([id]),
    CONSTRAINT [FK_proposals_sales_offices] FOREIGN KEY ([sales_office_id]) REFERENCES [dbo].[sales_offices] ([id]),
    CONSTRAINT [FK_proposals_spot_lengths] FOREIGN KEY ([default_spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IX_proposal_flight]
    ON [dbo].[proposals]([start_date] ASC, [end_date] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'original_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'original_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'version_number';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'version_number';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'product_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'agency_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'agency_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'advertiser_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'advertiser_company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'default_spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'default_spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'default_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'default_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'guarantee_type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 = House Holds, 1 = Demo', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'guarantee_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'guarantee_type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'proposal_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'proposal_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'proposal_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'proposal_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'traffic_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'traffic_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'budget';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'budget';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_equivalized';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_equivalized';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_audience_deficiency_unit_schedule';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_audience_deficiency_unit_schedule';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'include_on_availability_planner';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'include_on_availability_planner';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_dayparts';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_flights';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_flights';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_spot_lengths';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'multiple_spot_lengths';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'flight_text';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'optimization_goal';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'optimization_goal';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'optimization_target';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'optimization_target';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'billing_terms_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'billing_terms_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'additional_information';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'additional_information';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'salesperson_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'salesperson_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'date_last_modified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'network_rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'network_rate_card_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'buyer_note';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'buyer_note';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'print_title';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'print_title';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'include_on_marriage_report';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'include_on_marriage_report';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'exclude_from_pending_report';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'exclude_from_pending_report';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'posting_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'posting_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'billing_address_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'billing_address_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'confirmation_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'confirmation_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'primary_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'primary_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_msa';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_msa';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'number_of_materials';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'number_of_materials';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'total_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'total_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'total_gross_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'total_gross_cost';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_upfront';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'is_upfront';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'local_pods_authorized';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'local_pods_authorized';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'posting_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'posting_notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'media_plan_format_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposals', @level2type = N'COLUMN', @level2name = N'media_plan_format_code';


GO
CREATE TRIGGER dbo.tr_proposals ON dbo.proposals
FOR INSERT, UPDATE
AS
BEGIN

	UPDATE p 
		set p.agency_company_id =  cp.agency_company_id
			from proposals p (NOLOCK) 
			join proposal_proposals pp (NOLOCK) on pp.parent_proposal_id = p.id  
			join proposals cp (NOLOCK) on cp.id = pp.child_proposal_id and pp.ordinal = 0
		where p.agency_company_id = 0

END
GO
DISABLE TRIGGER [dbo].[tr_proposals]
    ON [dbo].[proposals];

