CREATE TABLE [dbo].[release_system_settings] (
    [release_id]              INT NOT NULL,
    [system_id]               INT NOT NULL,
    [is_on_financial_reports] BIT NOT NULL,
    CONSTRAINT [PK_release_system_settings] PRIMARY KEY CLUSTERED ([release_id] ASC, [system_id] ASC),
    CONSTRAINT [FK_release_system_settings_release] FOREIGN KEY ([release_id]) REFERENCES [dbo].[releases] ([id]),
    CONSTRAINT [FK_release_system_settings_system] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id])
);

