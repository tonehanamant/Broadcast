CREATE TABLE [dbo].[rule_dayparts] (
    [rule_id]    INT NOT NULL,
    [daypart_id] INT NOT NULL,
    CONSTRAINT [PK_rule_dayparts] PRIMARY KEY CLUSTERED ([rule_id] ASC, [daypart_id] ASC),
    CONSTRAINT [FK_rule_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_rule_dayparts_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id])
);

