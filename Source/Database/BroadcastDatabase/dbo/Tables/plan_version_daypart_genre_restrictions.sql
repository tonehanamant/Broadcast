CREATE TABLE [dbo].[plan_version_daypart_genre_restrictions] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_id] INT NOT NULL,
    [genre_id]                INT NOT NULL,
    CONSTRAINT [PK_plan_version_daypart_genre_restrictions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_daypart_genre_restrictions_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_plan_version_daypart_genre_restrictions_plan_version_dayparts] FOREIGN KEY ([plan_version_daypart_id]) REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_genre_restrictions_genres]
    ON [dbo].[plan_version_daypart_genre_restrictions]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_genre_restrictions_plan_version_dayparts]
    ON [dbo].[plan_version_daypart_genre_restrictions]([plan_version_daypart_id] ASC);

