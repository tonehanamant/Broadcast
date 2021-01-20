CREATE TABLE [dbo].[nti_universe_audience_mappings] (
    [id]                INT          IDENTITY (1, 1) NOT NULL,
    [audience_id]       INT          NOT NULL,
    [nti_audience_code] VARCHAR (15) NOT NULL,
    CONSTRAINT [PK_nti_universe_audience_mappings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_universe_audience_mappings_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_universe_audience_mappings_audiences]
    ON [dbo].[nti_universe_audience_mappings]([audience_id] ASC);

