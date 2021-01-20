CREATE TABLE [dbo].[spot_lengths] (
    [id]                  INT        IDENTITY (1, 1) NOT NULL,
    [length]              INT        NOT NULL,
    [delivery_multiplier] FLOAT (53) NOT NULL,
    [order_by]            INT        NOT NULL,
    [is_default]          BIT        NOT NULL,
    CONSTRAINT [PK_spot_lengths] PRIMARY KEY CLUSTERED ([id] ASC)
);

