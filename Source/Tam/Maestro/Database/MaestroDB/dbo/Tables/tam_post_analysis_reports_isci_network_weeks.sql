CREATE TABLE [dbo].[tam_post_analysis_reports_isci_network_weeks] (
    [tam_post_proposal_id] INT        NOT NULL,
    [audience_id]          INT        NOT NULL,
    [network_id]           INT        NOT NULL,
    [media_week_id]        INT        NOT NULL,
    [material_id]          INT        NOT NULL,
    [enabled]              BIT        NOT NULL,
    [subscribers]          BIGINT     NOT NULL,
    [delivery]             FLOAT (53) NOT NULL,
    [eq_delivery]          FLOAT (53) NOT NULL,
    [units]                FLOAT (53) NOT NULL,
    [dr_delivery]          FLOAT (53) NOT NULL,
    [dr_eq_delivery]       FLOAT (53) NOT NULL,
    [total_spots]          INT        NULL,
    CONSTRAINT [PK_tam_post_analysis_isci_network_weeks] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [audience_id] ASC, [network_id] ASC, [media_week_id] ASC, [material_id] ASC, [enabled] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_analysis_isci_network_weeks_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_analysis_isci_network_weeks_materials] FOREIGN KEY ([material_id]) REFERENCES [dbo].[materials] ([id]),
    CONSTRAINT [FK_tam_post_analysis_isci_network_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_tam_post_analysis_isci_network_weeks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_analysis_isci_network_weeks_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_tam_post_analysis_isci_network_weeks]
    ON [dbo].[tam_post_analysis_reports_isci_network_weeks]([tam_post_proposal_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'total_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_isci_network_weeks', @level2type = N'COLUMN', @level2name = N'total_spots';

