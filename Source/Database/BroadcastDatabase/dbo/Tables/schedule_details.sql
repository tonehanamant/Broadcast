CREATE TABLE [dbo].[schedule_details] (
    [id]             INT             IDENTITY (1, 1) NOT NULL,
    [schedule_id]    INT             NOT NULL,
    [market]         VARCHAR (127)   NOT NULL,
    [network]        VARCHAR (63)    NOT NULL,
    [program]        VARCHAR (127)   NOT NULL,
    [daypart_id]     INT             NOT NULL,
    [total_cost]     DECIMAL (10, 2) NOT NULL,
    [total_spots]    INT             NOT NULL,
    [spot_cost]      DECIMAL (10, 2) NOT NULL,
    [spot_length]    VARCHAR (7)     NULL,
    [spot_length_id] INT             NULL,
    CONSTRAINT [PK_schedule_details] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedule_details_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_schedule_details_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[schedules] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_schedule_details_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_details_spot_lengths]
    ON [dbo].[schedule_details]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_details_dayparts]
    ON [dbo].[schedule_details]([daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_details_schedule]
    ON [dbo].[schedule_details]([schedule_id] ASC);

