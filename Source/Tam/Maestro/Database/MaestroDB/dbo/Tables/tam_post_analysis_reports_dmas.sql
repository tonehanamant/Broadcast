CREATE TABLE [dbo].[tam_post_analysis_reports_dmas] (
    [tam_post_proposal_id]       INT        NOT NULL,
    [audience_id]                INT        NOT NULL,
    [enabled]                    BIT        NOT NULL,
    [subscribers_dma_1_10]       BIGINT     NOT NULL,
    [subscribers_dma_1_25]       BIGINT     NOT NULL,
    [subscribers_dma_1_50]       BIGINT     NOT NULL,
    [subscribers_dma_51_100]     BIGINT     NOT NULL,
    [subscribers_dma_101_210]    BIGINT     NOT NULL,
    [subscribers_dma_all]        BIGINT     NOT NULL,
    [delivery_dma_1_10]          FLOAT (53) NOT NULL,
    [delivery_dma_1_25]          FLOAT (53) NOT NULL,
    [delivery_dma_1_50]          FLOAT (53) NOT NULL,
    [delivery_dma_51_100]        FLOAT (53) NOT NULL,
    [delivery_dma_101_210]       FLOAT (53) NOT NULL,
    [delivery_dma_all]           FLOAT (53) NOT NULL,
    [eq_delivery_dma_1_10]       FLOAT (53) NOT NULL,
    [eq_delivery_dma_1_25]       FLOAT (53) NOT NULL,
    [eq_delivery_dma_1_50]       FLOAT (53) NOT NULL,
    [eq_delivery_dma_51_100]     FLOAT (53) NOT NULL,
    [eq_delivery_dma_101_210]    FLOAT (53) NOT NULL,
    [eq_delivery_dma_all]        FLOAT (53) NOT NULL,
    [dr_delivery_dma_1_10]       FLOAT (53) NOT NULL,
    [dr_delivery_dma_1_25]       FLOAT (53) NOT NULL,
    [dr_delivery_dma_1_50]       FLOAT (53) NOT NULL,
    [dr_delivery_dma_51_100]     FLOAT (53) NOT NULL,
    [dr_delivery_dma_101_210]    FLOAT (53) NOT NULL,
    [dr_delivery_dma_all]        FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_1_10]    FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_1_25]    FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_1_50]    FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_51_100]  FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_101_210] FLOAT (53) NOT NULL,
    [dr_eq_delivery_dma_all]     FLOAT (53) NOT NULL,
    [units_dma_1_10]             FLOAT (53) NOT NULL,
    [units_dma_1_25]             FLOAT (53) NOT NULL,
    [units_dma_1_50]             FLOAT (53) NOT NULL,
    [units_dma_51_100]           FLOAT (53) NOT NULL,
    [units_dma_101_210]          FLOAT (53) NOT NULL,
    [units_dma_all]              FLOAT (53) NOT NULL,
    CONSTRAINT [PK_tam_post_analysis_reports_dma] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [audience_id] ASC, [enabled] ASC),
    CONSTRAINT [FK_tam_post_analysis_reports_dma_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_dma_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_tam_post_analysis_reports_dmas]
    ON [dbo].[tam_post_analysis_reports_dmas]([tam_post_proposal_id] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the DMA by range post analysis reports.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Analysis Report - DMA Range', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the posting plan and post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Whether or not the data counts towards the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'When false, the data has been excluded from the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'subscribers_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'units_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'eq_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic for all DMAs.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_delivery_dma_all';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for DMAs 1-10.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_10';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for DMAs 1-25.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_25';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for DMAs 1-50.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_1_50';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for DMAs 51-100.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_51_100';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic for DMAs 101-210.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_101_210';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_dmas', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery_dma_101_210';

