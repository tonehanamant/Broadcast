CREATE TABLE [dbo].[audience_maps] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [audience_id] INT           NOT NULL,
    [map_value]   NVARCHAR (16) NOT NULL,
    CONSTRAINT [PK_audience_maps] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_audience_maps_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_audience_maps_audiences]
    ON [dbo].[audience_maps]([audience_id] ASC);

