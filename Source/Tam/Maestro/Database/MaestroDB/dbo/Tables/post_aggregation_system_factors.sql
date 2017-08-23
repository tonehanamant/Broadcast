CREATE TABLE [dbo].[post_aggregation_system_factors] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [name]           VARCHAR (63) NOT NULL,
    [system_id]      INT          NOT NULL,
    [spot_length_id] INT          NOT NULL,
    [operator]       TINYINT      NOT NULL,
    [factor]         FLOAT (53)   NOT NULL,
    [start_date]     DATETIME     NOT NULL,
    [end_date]       DATETIME     NULL,
    CONSTRAINT [PK_post_aggregation_system_factors] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_post_aggregation_system_factors_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_post_aggregation_system_factors_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [IX_post_aggregation_system_factors] UNIQUE NONCLUSTERED ([system_id] ASC, [spot_length_id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Effective end date of this factor.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains special rules for handling specific systems ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Post Aggregation System Factors', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used internally in the posting algorithm to apply special factors to delivery by system and spot length.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Useful name for this factor.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the system.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the spot length.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The tye of operation.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'operator';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Multiply=0, Divide=1', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'operator';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The factor to apply to delivery.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'factor';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'factor';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Effective start date of this factor.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'post_aggregation_system_factors', @level2type = N'COLUMN', @level2name = N'start_date';

