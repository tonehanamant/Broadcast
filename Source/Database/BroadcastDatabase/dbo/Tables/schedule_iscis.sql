CREATE TABLE [dbo].[schedule_iscis] (
    [id]          INT           IDENTITY (1, 1) NOT NULL,
    [schedule_id] INT           NOT NULL,
    [house_isci]  VARCHAR (63)  NOT NULL,
    [client_isci] VARCHAR (63)  NOT NULL,
    [brand]       VARCHAR (127) NULL,
    CONSTRAINT [PK_schedule_iscis] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedule_iscis_schedules] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[schedules] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_iscis_schedules]
    ON [dbo].[schedule_iscis]([schedule_id] ASC);

