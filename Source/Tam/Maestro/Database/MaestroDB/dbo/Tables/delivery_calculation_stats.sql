CREATE TABLE [dbo].[delivery_calculation_stats] (
    [date_run]                         DATETIME NOT NULL,
    [media_month_id]                   INT      NOT NULL,
    [rating_source_id]                 TINYINT  NOT NULL,
    [num_needed_affidavits]            INT      NOT NULL,
    [num_done_affidavits]              INT      NOT NULL,
    [num_remaining_affidavits]         INT      NOT NULL,
    [time_to_get_needed_affidavits]    INT      NOT NULL,
    [time_to_get_done_affidavits]      INT      NOT NULL,
    [time_to_get_remaining_affidavits] INT      NOT NULL,
    [time_to_calculate_deliveries]     INT      NOT NULL,
    [date_completed]                   DATETIME NULL,
    CONSTRAINT [PK_delivery_calculation_stats] PRIMARY KEY CLUSTERED ([date_run] ASC, [media_month_id] ASC, [rating_source_id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'date_run';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'date_run';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'media_month_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'rating_source_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'num_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_needed_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_done_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_get_remaining_affidavits';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_calculate_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'time_to_calculate_deliveries';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'date_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'delivery_calculation_stats', @level2type = N'COLUMN', @level2name = N'date_completed';

