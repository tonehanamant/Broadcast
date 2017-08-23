CREATE TABLE [dbo].[network_genres] (
    [network_id]     INT      NOT NULL,
    [genre_id]       INT      NOT NULL,
    [effective_date] DATETIME NOT NULL,
    CONSTRAINT [PK_network_genres] PRIMARY KEY CLUSTERED ([network_id] ASC, [genre_id] ASC),
    CONSTRAINT [FK_network_genres_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_network_genres_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);

