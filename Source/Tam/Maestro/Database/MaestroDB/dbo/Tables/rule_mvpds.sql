CREATE TABLE [dbo].[rule_mvpds] (
    [rule_id] INT NOT NULL,
    [mvpd_id] INT NOT NULL,
    CONSTRAINT [PK_rule_mvpds] PRIMARY KEY CLUSTERED ([rule_id] ASC, [mvpd_id] ASC),
    CONSTRAINT [FK_rule_mvpds_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_rule_mvpds_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id])
);

