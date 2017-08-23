CREATE TABLE [dbo].[proposal_inventory_checks] (
    [proposal_id]                  INT        NOT NULL,
    [date_created]                 DATETIME   NOT NULL,
    [proposal_status_id]           INT        NOT NULL,
    [base_media_month_id]          INT        NOT NULL,
    [health_score]                 FLOAT (53) NOT NULL,
    [total_allocated_subscribers]  BIGINT     NOT NULL,
    [total_contracted_subscribers] BIGINT     NOT NULL,
    [total_forecasted_subscribers] BIGINT     NOT NULL,
    [ims_duration]                 INT        NOT NULL,
    CONSTRAINT [PK_proposal_inventory_checks] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [date_created] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_proposal_inventory_checks_media_months] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_inventory_checks_proposal_statuses] FOREIGN KEY ([proposal_status_id]) REFERENCES [dbo].[proposal_statuses] ([id]),
    CONSTRAINT [FK_proposal_inventory_checks_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

