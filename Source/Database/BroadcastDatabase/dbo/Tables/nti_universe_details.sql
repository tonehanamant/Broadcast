CREATE TABLE [dbo].[nti_universe_details] (
    [id]                     INT          IDENTITY (1, 1) NOT NULL,
    [nti_universe_header_id] INT          NOT NULL,
    [nti_audience_id]        INT          NOT NULL,
    [nti_audience_code]      VARCHAR (15) NOT NULL,
    [universe]               FLOAT (53)   NOT NULL,
    CONSTRAINT [PK_nti_universe_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_universe_details_nti_universe_headers] FOREIGN KEY ([nti_universe_header_id]) REFERENCES [dbo].[nti_universe_headers] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_universe_details_nti_universe_headers]
    ON [dbo].[nti_universe_details]([nti_universe_header_id] ASC);

