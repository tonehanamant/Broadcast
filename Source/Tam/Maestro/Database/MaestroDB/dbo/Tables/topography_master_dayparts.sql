CREATE TABLE [dbo].[topography_master_dayparts] (
    [topography_id]  INT NOT NULL,
    [daypart_id]     INT NOT NULL,
    [can_be_altered] BIT NOT NULL,
    PRIMARY KEY CLUSTERED ([topography_id] ASC, [daypart_id] ASC),
    CONSTRAINT [FK_topography_master_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_topography_master_dayparts_topographies] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);

