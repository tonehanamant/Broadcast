CREATE TABLE [dbo].[plan_version_daypart_show_type_restrictions] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_id] INT NOT NULL,
    [show_type_id]            INT NOT NULL,
    CONSTRAINT [PK_plan_version_daypart_show_type_restrictions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_plan_version_dayparts] FOREIGN KEY ([plan_version_daypart_id]) REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_daypart_show_type_restrictions_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_show_type_restrictions_plan_version_dayparts]
    ON [dbo].[plan_version_daypart_show_type_restrictions]([plan_version_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_show_type_restrictions_show_types]
    ON [dbo].[plan_version_daypart_show_type_restrictions]([show_type_id] ASC);

