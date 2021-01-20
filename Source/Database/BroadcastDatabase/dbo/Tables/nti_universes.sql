CREATE TABLE [dbo].[nti_universes] (
    [id]                     INT        IDENTITY (1, 1) NOT NULL,
    [audience_id]            INT        NOT NULL,
    [universe]               FLOAT (53) NOT NULL,
    [nti_universe_header_id] INT        NOT NULL,
    CONSTRAINT [PK_nti_universes] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_universes_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_nti_universes_nti_universe_headers] FOREIGN KEY ([nti_universe_header_id]) REFERENCES [dbo].[nti_universe_headers] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_universes_nti_universe_headers]
    ON [dbo].[nti_universes]([nti_universe_header_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_universes_audiences]
    ON [dbo].[nti_universes]([audience_id] ASC);

