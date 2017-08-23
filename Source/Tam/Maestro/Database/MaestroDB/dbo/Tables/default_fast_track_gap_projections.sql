CREATE TABLE [dbo].[default_fast_track_gap_projections] (
    [media_month_id]    INT        NOT NULL,
    [rate_card_type_id] INT        NOT NULL,
    [gap_projection]    FLOAT (53) NOT NULL,
    [lock_date]         DATETIME   NULL,
    CONSTRAINT [PK_default_fast_track_gap_projections] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [rate_card_type_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_default_fast_track_gap_projections_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_default_fast_track_gap_projections_rate_card_types] FOREIGN KEY ([rate_card_type_id]) REFERENCES [dbo].[rate_card_types] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'RateCardTypeEnum', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'rate_card_type_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'gap_projection';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'gap_projection';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'lock_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_fast_track_gap_projections', @level2type = N'COLUMN', @level2name = N'lock_date';

