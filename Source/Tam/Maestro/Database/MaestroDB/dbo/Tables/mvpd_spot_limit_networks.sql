CREATE TABLE [dbo].[mvpd_spot_limit_networks] (
    [mvpd_spot_limit_id] INT NOT NULL,
    [network_id]         INT NOT NULL,
    CONSTRAINT [PK_mvpd_spot_limit_networks] PRIMARY KEY CLUSTERED ([mvpd_spot_limit_id] ASC, [network_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_mvpd_spot_limit_networks_mvpd_spot_limits] FOREIGN KEY ([mvpd_spot_limit_id]) REFERENCES [dbo].[mvpd_spot_limits] ([id]),
    CONSTRAINT [FK_mvpd_spot_limit_networks_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id])
);

