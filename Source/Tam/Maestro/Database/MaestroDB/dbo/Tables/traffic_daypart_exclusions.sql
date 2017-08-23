CREATE TABLE [dbo].[traffic_daypart_exclusions] (
    [id]                 INT IDENTITY (1, 1) NOT NULL,
    [traffic_id]         INT NOT NULL,
    [start_time_seconds] INT DEFAULT (NULL) NULL,
    [end_time_seconds]   INT DEFAULT (NULL) NULL,
    [monday]             BIT DEFAULT ((0)) NOT NULL,
    [tuesday]            BIT DEFAULT ((0)) NOT NULL,
    [wednesday]          BIT DEFAULT ((0)) NOT NULL,
    [thursday]           BIT DEFAULT ((0)) NOT NULL,
    [friday]             BIT DEFAULT ((0)) NOT NULL,
    [saturday]           BIT DEFAULT ((0)) NOT NULL,
    [sunday]             BIT DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_traffic_daypart_exclusions] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_daypart_exclusions_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id])
);

