CREATE TABLE [dbo].[network_rate_card_book_topographies] (
    [network_rate_card_book_id] INT NOT NULL,
    [topography_id]             INT NOT NULL,
    CONSTRAINT [PK_network_rate_card_topographies] PRIMARY KEY CLUSTERED ([network_rate_card_book_id] ASC, [topography_id] ASC),
    CONSTRAINT [FK_network_rate_card_book_topographies_network_rate_card_books] FOREIGN KEY ([network_rate_card_book_id]) REFERENCES [dbo].[network_rate_card_books] ([id]),
    CONSTRAINT [FK_network_rate_card_book_topographies_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies', @level2type = N'COLUMN', @level2name = N'network_rate_card_book_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies', @level2type = N'COLUMN', @level2name = N'network_rate_card_book_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'network_rate_card_book_topographies', @level2type = N'COLUMN', @level2name = N'topography_id';

