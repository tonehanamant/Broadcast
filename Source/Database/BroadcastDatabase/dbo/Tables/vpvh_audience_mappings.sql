CREATE TABLE [dbo].[vpvh_audience_mappings] (
    [id]                  INT IDENTITY (1, 1) NOT NULL,
    [audience_id]         INT NOT NULL,
    [compose_audience_id] INT NOT NULL,
    [operation]           INT NOT NULL,
    CONSTRAINT [PK_vpvh_audience_mappings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_vpvh_audience_mapping_audiences_compose_audiences] FOREIGN KEY ([compose_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_vpvh_audience_mappings_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_vpvh_audience_mappings_audiences]
    ON [dbo].[vpvh_audience_mappings]([audience_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_vpvh_audience_mapping_audiences_compose_audiences]
    ON [dbo].[vpvh_audience_mappings]([compose_audience_id] ASC);

