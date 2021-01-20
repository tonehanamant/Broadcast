CREATE TABLE [dbo].[spot_length_cost_multipliers] (
    [id]              INT        IDENTITY (1, 1) NOT NULL,
    [spot_length_id]  INT        NOT NULL,
    [cost_multiplier] FLOAT (53) NOT NULL,
    CONSTRAINT [PK_spot_length_cost_multipliers] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_spot_length_cost_multipliers_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_spot_length_cost_multipliers_spot_lengths]
    ON [dbo].[spot_length_cost_multipliers]([spot_length_id] ASC);

