CREATE TABLE [dbo].[mvpd_cancellation_deadlines] (
    [mvpd_id]              INT           NOT NULL,
    [flight_start_day]     SMALLINT      NOT NULL,
    [cacellation_deadline] NVARCHAR (50) NOT NULL,
    [days_limit]           SMALLINT      NOT NULL,
    CONSTRAINT [PK_mvpd_cancellation_deadlines] PRIMARY KEY CLUSTERED ([mvpd_id] ASC, [flight_start_day] ASC),
    CONSTRAINT [FK_mvpd_cancellation_deadlines_businesses] FOREIGN KEY ([mvpd_id]) REFERENCES [dbo].[businesses] ([id])
);

