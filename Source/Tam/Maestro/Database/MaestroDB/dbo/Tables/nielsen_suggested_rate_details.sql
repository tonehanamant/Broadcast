CREATE TABLE [dbo].[nielsen_suggested_rate_details] (
    [id]                        INT        IDENTITY (1, 1) NOT NULL,
    [nielsen_suggested_rate_id] INT        NOT NULL,
    [network_id]                INT        NOT NULL,
    [daypart_id]                INT        NOT NULL,
    [suggested_cpm]             MONEY      NOT NULL,
    [total_spend]               MONEY      NOT NULL,
    [total_delivery]            FLOAT (53) NOT NULL,
    [network_group_type]        TINYINT    NOT NULL,
    CONSTRAINT [PK_nielsen_suggested_rate_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nielsen_suggested_rate_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_nielsen_suggested_rate_details_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_nielsen_suggested_rate_details_nielsen_suggested_rates] FOREIGN KEY ([nielsen_suggested_rate_id]) REFERENCES [dbo].[nielsen_suggested_rates] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'nielsen_suggested_rate_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'nielsen_suggested_rate_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'suggested_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'suggested_cpm';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'total_spend';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'total_spend';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'total_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'total_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'network_group_type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'nielsen_suggested_rate_details', @level2type = N'COLUMN', @level2name = N'network_group_type';

