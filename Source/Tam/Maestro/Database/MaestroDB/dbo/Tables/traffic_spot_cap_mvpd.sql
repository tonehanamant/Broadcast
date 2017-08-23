CREATE TABLE [dbo].[traffic_spot_cap_mvpd] (
    [id]                  INT IDENTITY (1, 1) NOT NULL,
    [traffic_spot_cap_id] INT NOT NULL,
    [mvpd_id]             INT NOT NULL,
    CONSTRAINT [PK_traffic_spot_cap_mvpd] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_traffic_spot_cap_mvpd_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id]),
    CONSTRAINT [FK_traffic_spot_cap_mvpd_traffic_spot_cap] FOREIGN KEY ([traffic_spot_cap_id]) REFERENCES [dbo].[traffic_spot_cap] ([id])
);

