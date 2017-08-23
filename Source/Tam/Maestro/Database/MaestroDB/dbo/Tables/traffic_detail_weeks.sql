CREATE TABLE [dbo].[traffic_detail_weeks] (
    [id]                            INT      IDENTITY (1, 1) NOT NULL,
    [traffic_detail_id]             INT      NOT NULL,
    [start_date]                    DATETIME NOT NULL,
    [end_date]                      DATETIME NOT NULL,
    [suspended]                     BIT      NOT NULL,
    [suspended_by_traffic_alert_id] INT      NULL,
    CONSTRAINT [PK_traffic_detail_weeks_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_detail_weeks_traffic_details] FOREIGN KEY ([traffic_detail_id]) REFERENCES [dbo].[traffic_details] ([id]),
    CONSTRAINT [IX_traffic_detail_weeks] UNIQUE NONCLUSTERED ([traffic_detail_id] ASC, [start_date] ASC) WITH (FILLFACTOR = 90)
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Unique ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The corresponding traffic detail record which defines a network in a traffic order.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The start date of the week for this particular traffic detail record, always falls in a media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The end date of the week for this particular traffic detail record, always falls in a media week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'suspended';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'A boolean, if true, the week has been suspended after run, if false, it''s an active week.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'suspended';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_weeks', @level2type = N'COLUMN', @level2name = N'suspended';

