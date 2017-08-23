CREATE TABLE [dbo].[affidavit_deliveries] (
    [media_month_id]   SMALLINT   NOT NULL,
    [rating_source_id] TINYINT    NOT NULL,
    [audience_id]      INT        NOT NULL,
    [affidavit_id]     BIGINT     NOT NULL,
    [audience_usage]   FLOAT (53) NOT NULL,
    [universe]         FLOAT (53) NOT NULL,
    [regional_usage]   FLOAT (53) NULL,
    [regional_rating]  FLOAT (53) NULL,
    CONSTRAINT [PK_p_affidavit_deliveries] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [rating_source_id] ASC, [audience_id] ASC, [affidavit_id] ASC) ON [MediaMonthSmallintScheme] ([media_month_id])
);


GO
CREATE NONCLUSTERED INDEX [IX_p_affidavit_deliveries_affidavit_id_]
    ON [dbo].[affidavit_deliveries]([affidavit_id] ASC)
    ON [MediaMonthSmallintScheme] ([media_month_id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'affidavit_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'audience_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'regional_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'regional_usage';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'regional_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_deliveries', @level2type = N'COLUMN', @level2name = N'regional_rating';

