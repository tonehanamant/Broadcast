CREATE TABLE [dbo].[plan_version_daypart_program_restrictions] (
    [id]                      INT           IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_id] INT           NOT NULL,
    [program_name]            VARCHAR (255) NULL,
    [genre_id]                INT           NULL,
    [content_rating]          VARCHAR (15)  NULL,
    CONSTRAINT [PK_plan_version_daypart_program_restrictions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK__plan_vers__genre__6CAFA246] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_plan_version_daypart_program_restrictions_plan_version_dayparts] FOREIGN KEY ([plan_version_daypart_id]) REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK__plan_vers__genre__6CAFA246]
    ON [dbo].[plan_version_daypart_program_restrictions]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_program_restrictions_plan_version_dayparts]
    ON [dbo].[plan_version_daypart_program_restrictions]([plan_version_daypart_id] ASC);

