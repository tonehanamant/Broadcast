CREATE TABLE [dbo].[rule_networks] (
    [rule_id]    INT NOT NULL,
    [network_id] INT NOT NULL,
    CONSTRAINT [PK_rule_networks] PRIMARY KEY CLUSTERED ([rule_id] ASC, [network_id] ASC),
    CONSTRAINT [FK_rule_networks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_rule_networks_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id])
);

