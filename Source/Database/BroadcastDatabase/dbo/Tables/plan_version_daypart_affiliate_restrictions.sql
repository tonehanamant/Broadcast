CREATE TABLE [dbo].[plan_version_daypart_affiliate_restrictions] (
    [id]                      INT IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_id] INT NOT NULL,
    [affiliate_id]            INT NOT NULL,
    CONSTRAINT [PK_plan_version_daypart_affiliate_restrictions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_affiliates] FOREIGN KEY ([affiliate_id]) REFERENCES [dbo].[affiliates] ([id]),
    CONSTRAINT [FK_plan_version_daypart_affiliate_restrictions_plan_version_dayparts] FOREIGN KEY ([plan_version_daypart_id]) REFERENCES [dbo].[plan_version_dayparts] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_affiliate_restrictions_plan_version_dayparts]
    ON [dbo].[plan_version_daypart_affiliate_restrictions]([plan_version_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_daypart_affiliate_restrictions_affiliates]
    ON [dbo].[plan_version_daypart_affiliate_restrictions]([affiliate_id] ASC);

