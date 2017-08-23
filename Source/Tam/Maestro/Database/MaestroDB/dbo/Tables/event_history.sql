CREATE TABLE [dbo].[event_history] (
    [id]                     INT            IDENTITY (1, 1) NOT NULL,
    [identifier]             NVARCHAR (100) NOT NULL,
    [event_type_id]          INT            NOT NULL,
    [details]                NVARCHAR (100) NOT NULL,
    [created_by]             INT            NULL,
    [created_time]           DATETIME       NOT NULL,
    [use_case_id]            INT            NULL,
    [additional_information] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_event_history] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_event_history_employees] FOREIGN KEY ([created_by]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_event_history_event_type] FOREIGN KEY ([event_type_id]) REFERENCES [dbo].[event_type] ([id]),
    CONSTRAINT [FK_event_history_use_cases] FOREIGN KEY ([use_case_id]) REFERENCES [dbo].[use_cases] ([id])
);

