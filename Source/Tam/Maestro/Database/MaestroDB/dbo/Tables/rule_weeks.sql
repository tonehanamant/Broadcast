CREATE TABLE [dbo].[rule_weeks] (
    [rule_id]       INT NOT NULL,
    [media_week_id] INT NOT NULL,
    CONSTRAINT [PK_rule_weeks] PRIMARY KEY CLUSTERED ([rule_id] ASC, [media_week_id] ASC),
    CONSTRAINT [FK_rule_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_rule_weeks_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id])
);

