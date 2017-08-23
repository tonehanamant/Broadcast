CREATE TABLE [dbo].[rule_dmas] (
    [rule_id] INT NOT NULL,
    [dma_id]  INT NOT NULL,
    CONSTRAINT [PK_rule_dmas] PRIMARY KEY CLUSTERED ([rule_id] ASC, [dma_id] ASC),
    CONSTRAINT [FK_rule_dmas_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id]),
    CONSTRAINT [FK_rule_dmas_rules] FOREIGN KEY ([rule_id]) REFERENCES [dbo].[rules] ([id])
);

