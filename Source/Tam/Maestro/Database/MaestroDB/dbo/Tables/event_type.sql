CREATE TABLE [dbo].[event_type] (
    [id]          INT            NOT NULL,
    [name]        NVARCHAR (100) NOT NULL,
    [event_group] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_event_history_type] PRIMARY KEY CLUSTERED ([id] ASC)
);

