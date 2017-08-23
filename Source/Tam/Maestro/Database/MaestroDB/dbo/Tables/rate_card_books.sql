CREATE TABLE [dbo].[rate_card_books] (
    [id]                           INT          IDENTITY (1, 1) NOT NULL,
    [name]                         VARCHAR (63) NOT NULL,
    [version]                      INT          NOT NULL,
    [rate_card_version_id]         INT          NOT NULL,
    [base_ratings_media_month_id]  INT          NOT NULL,
    [base_universe_media_month_id] INT          NOT NULL,
    [date_created]                 DATETIME     CONSTRAINT [DF_rate_card_books_fixed] DEFAULT ((0)) NOT NULL,
    [date_approved]                DATETIME     NULL,
    [effective_date]               DATETIME     NOT NULL,
    CONSTRAINT [PK_rate_card_books] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_rate_card_books_media_months] FOREIGN KEY ([base_ratings_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_rate_card_books_media_months1] FOREIGN KEY ([base_universe_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_rate_card_books_rate_card_versions] FOREIGN KEY ([rate_card_version_id]) REFERENCES [dbo].[rate_card_versions] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'version';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'version';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'rate_card_version_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'rate_card_version_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'base_ratings_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'base_universe_media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'date_created';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'date_approved';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'date_approved';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rate_card_books', @level2type = N'COLUMN', @level2name = N'effective_date';

