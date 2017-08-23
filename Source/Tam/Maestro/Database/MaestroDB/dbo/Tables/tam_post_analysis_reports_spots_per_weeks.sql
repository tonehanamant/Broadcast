CREATE TABLE [dbo].[tam_post_analysis_reports_spots_per_weeks] (
    [tam_post_proposal_id] INT        NOT NULL,
    [audience_id]          INT        NOT NULL,
    [network_id]           INT        NOT NULL,
    [media_week_id]        INT        NOT NULL,
    [enabled]              BIT        NOT NULL,
    [subscribers]          BIGINT     NOT NULL,
    [delivery]             FLOAT (53) NOT NULL,
    [eq_delivery]          FLOAT (53) NOT NULL,
    [units]                FLOAT (53) NOT NULL,
    [dr_delivery]          FLOAT (53) NOT NULL,
    [dr_eq_delivery]       FLOAT (53) NOT NULL,
    CONSTRAINT [PK_tam_post_analysis_reports_spots_per_week] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [audience_id] ASC, [network_id] ASC, [media_week_id] ASC, [enabled] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_analysis_reports_spots_per_week_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_spots_per_week_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_spots_per_week_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_spots_per_week_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_tam_post_analysis_reports_spots_per_weeks]
    ON [dbo].[tam_post_analysis_reports_spots_per_weeks]([tam_post_proposal_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized direct response delivery for this demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'dr_eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains the spots per week post analysis reports.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Analysis Report - Spots Per Week', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the posting plan and post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the network.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Whether or not the data counts towards the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'When false, the data has been excluded from the post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'enabled';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total subscribers for this aggregation.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'subscribers';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized delivery for this demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The equivalized delivery for this demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'eq_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total direct response units.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Only filled in for house holds demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The unequivalized direct response delivery for this demographic.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'dr_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_analysis_reports_spots_per_weeks', @level2type = N'COLUMN', @level2name = N'dr_delivery';

