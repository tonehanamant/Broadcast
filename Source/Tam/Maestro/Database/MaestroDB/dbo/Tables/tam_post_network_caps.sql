CREATE TABLE [dbo].[tam_post_network_caps] (
    [tam_post_id]                     INT        NOT NULL,
    [network_id]                      INT        NOT NULL,
    [network_delivery_cap_percentage] FLOAT (53) NULL,
    [bonus]                           BIT        NOT NULL,
    CONSTRAINT [PK_tam_post_network_caps] PRIMARY KEY CLUSTERED ([tam_post_id] ASC, [network_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_network_caps_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_network_caps_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'network_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'network_delivery_cap_percentage';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_network_caps', @level2type = N'COLUMN', @level2name = N'network_delivery_cap_percentage';

