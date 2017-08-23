CREATE TABLE [dbo].[affidavit_delivery_runs] (
    [id]                               INT      IDENTITY (1, 1) NOT NULL,
    [media_month_id]                   INT      NOT NULL,
    [rating_source_id]                 TINYINT  NOT NULL,
    [started_by_employee_id]           INT      NOT NULL,
    [status_code]                      TINYINT  NOT NULL,
    [total_needed_affidavits]          INT      NOT NULL,
    [total_done_affidavits]            INT      NOT NULL,
    [total_remaining_affidavits]       INT      NOT NULL,
    [time_to_get_needed_affidavits]    INT      NOT NULL,
    [time_to_get_done_affidavits]      INT      NOT NULL,
    [time_to_get_remaining_affidavits] INT      NOT NULL,
    [time_to_calculate_deliveries]     INT      NOT NULL,
    [num_processed]                    INT      NOT NULL,
    [num_remaining]                    INT      NOT NULL,
    [date_queued]                      DATETIME NOT NULL,
    [date_started]                     DATETIME NULL,
    [date_last_updated]                DATETIME NOT NULL,
    [date_completed]                   DATETIME NULL,
    CONSTRAINT [PK_affidavit_delivery_runs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_affidavit_delivery_runs_employees] FOREIGN KEY ([started_by_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_affidavit_delivery_runs_media_months] FOREIGN KEY ([media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_affidavit_delivery_runs_rating_sources] FOREIGN KEY ([rating_source_id]) REFERENCES [dbo].[rating_sources] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'ERatingsSource', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'started_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'started_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'EAffidavitDeliveryRunStatus', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'total_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_get_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_calculate_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'time_to_calculate_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'num_processed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'num_processed';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'num_remaining';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'num_remaining';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_queued';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_queued';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_started';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_started';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_last_updated';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_last_updated';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'affidavit_delivery_runs', @level2type = N'COLUMN', @level2name = N'date_completed';

