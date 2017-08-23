CREATE TABLE [dbo].[rotational_biases] (
    [id]                 INT        IDENTITY (1, 1) NOT NULL,
    [media_month_id]     INT        NOT NULL,
    [daypart_id]         INT        NOT NULL,
    [nielsen_network_id] INT        NOT NULL,
    [audience_id]        INT        NOT NULL,
    [spot_length_id]     INT        NOT NULL,
    [bias]               FLOAT (53) NOT NULL,
    [rating_category_id] INT        DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_rotational_bias] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_rotational_bias_2_audience] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_rotational_bias_2_daypart] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_rotational_bias_2_media_month] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_rotational_bias_2_nielsen_network] FOREIGN KEY ([nielsen_network_id]) REFERENCES [dbo].[nielsen_networks] ([id]),
    CONSTRAINT [FK_rotational_bias_2_rating_category] FOREIGN KEY ([rating_category_id]) REFERENCES [dbo].[rating_categories] ([id]),
    CONSTRAINT [FK_rotational_bias_2_spot_length] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [SK_ratational_bias]
    ON [dbo].[rotational_biases]([media_month_id] ASC, [daypart_id] ASC, [nielsen_network_id] ASC, [audience_id] ASC, [spot_length_id] ASC, [rating_category_id] ASC) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'bias';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'bias';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'rating_category_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'rotational_biases', @level2type = N'COLUMN', @level2name = N'rating_category_id';

