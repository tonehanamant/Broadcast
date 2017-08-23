CREATE TABLE [dbo].[traffic_mvpd_ratings] (
    [id]              INT        IDENTITY (1, 1) NOT NULL,
    [traffic_id]      INT        NOT NULL,
    [mvpd_id]         INT        NOT NULL,
    [network_id]      INT        NOT NULL,
    [daypart_id]      INT        NOT NULL,
    [audience_id]     INT        NOT NULL,
    [proposal_rating] FLOAT (53) NOT NULL,
    [traffic_rating]  FLOAT (53) NOT NULL,
    [scaling_factor]  FLOAT (53) NOT NULL,
    CONSTRAINT [PK_traffic_mvpd_ratings] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_mvpd_ratings_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_mvpd_ratings_traffic__id]
    ON [dbo].[traffic_mvpd_ratings]([traffic_id] ASC);

