CREATE TABLE [dbo].[network_genre_histories] (
    [network_id] INT      NOT NULL,
    [genre_id]   INT      NOT NULL,
    [start_date] DATETIME NOT NULL,
    [end_date]   DATETIME NOT NULL,
    CONSTRAINT [PK_network_genre_histories] PRIMARY KEY CLUSTERED ([network_id] ASC, [genre_id] ASC, [start_date] ASC),
    CONSTRAINT [FK_network_genre_histories_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_network_genre_histories_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);

