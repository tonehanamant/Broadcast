CREATE TABLE [dbo].[cluster_rate_card_details] (
    [id]                   INT IDENTITY (1, 1) NOT NULL,
    [cluster_rate_card_id] INT NOT NULL,
    [daypart_id]           INT NOT NULL,
    [cluster_id]           INT NULL,
    CONSTRAINT [PK_cluster_rate_card_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_cluster_rate_card_details_cluster_rate_cards] FOREIGN KEY ([cluster_rate_card_id]) REFERENCES [dbo].[cluster_rate_cards] ([id]),
    CONSTRAINT [FK_cluster_rate_card_details_clusters] FOREIGN KEY ([cluster_id]) REFERENCES [dbo].[clusters] ([id]),
    CONSTRAINT [FK_cluster_rate_card_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id])
);


GO
ALTER TABLE [dbo].[cluster_rate_card_details] NOCHECK CONSTRAINT [FK_cluster_rate_card_details_dayparts];

