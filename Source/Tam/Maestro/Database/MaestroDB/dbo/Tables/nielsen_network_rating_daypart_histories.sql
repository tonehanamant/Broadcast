CREATE TABLE [dbo].[nielsen_network_rating_daypart_histories] (
    [nielsen_network_id] INT      NOT NULL,
    [daypart_id]         INT      NOT NULL,
    [start_date]         DATETIME NOT NULL,
    [end_date]           DATETIME NOT NULL,
    CONSTRAINT [PK_nielson_network_rating_daypart_histories] PRIMARY KEY CLUSTERED ([nielsen_network_id] ASC, [daypart_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_nielson_network_rating_daypart_histories_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_nielson_network_rating_daypart_histories_nielson_networks] FOREIGN KEY ([nielsen_network_id]) REFERENCES [dbo].[nielsen_networks] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'nielsen_network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_network_rating_daypart_histories', @level2type = N'COLUMN', @level2name = N'end_date';

