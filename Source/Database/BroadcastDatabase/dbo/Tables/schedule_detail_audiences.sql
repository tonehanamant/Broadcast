CREATE TABLE [dbo].[schedule_detail_audiences] (
    [id]                  INT        IDENTITY (1, 1) NOT NULL,
    [schedule_detail_id]  INT        NOT NULL,
    [audience_id]         INT        NOT NULL,
    [impressions]         FLOAT (53) NOT NULL,
    [audience_rank]       INT        NOT NULL,
    [audience_population] INT        NOT NULL,
    CONSTRAINT [PK_schedule_detail_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_schedule_detail_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_schedule_detail_audiences_schedule_details] FOREIGN KEY ([schedule_detail_id]) REFERENCES [dbo].[schedule_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_detail_audiences_audiences]
    ON [dbo].[schedule_detail_audiences]([audience_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_schedule_detail_audiences_schedule_details]
    ON [dbo].[schedule_detail_audiences]([schedule_detail_id] ASC);

