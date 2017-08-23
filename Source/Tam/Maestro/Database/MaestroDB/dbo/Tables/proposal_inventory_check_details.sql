CREATE TABLE [dbo].[proposal_inventory_check_details] (
    [proposal_id]                                  INT        NOT NULL,
    [proposal_detail_id]                           INT        NOT NULL,
    [date_created]                                 DATETIME   NOT NULL,
    [proposal_status_id]                           INT        NOT NULL,
    [base_media_month_id]                          INT        NOT NULL,
    [spot_length_id]                               TINYINT    NOT NULL,
    [network_id]                                   INT        NOT NULL,
    [daypart_id]                                   INT        NOT NULL,
    [rating_daypart_id]                            INT        NOT NULL,
    [proposal_rate]                                MONEY      NOT NULL,
    [coverage_universe]                            INT        NOT NULL,
    [hh_eq_cpm]                                    MONEY      NOT NULL,
    [health_score]                                 FLOAT (53) NOT NULL,
    [total_contracted_subscribers]                 BIGINT     NOT NULL,
    [total_contracted_units]                       INT        NOT NULL,
    [total_allocated_subscribers]                  BIGINT     NOT NULL,
    [total_allocated_units]                        FLOAT (53) NOT NULL,
    [total_forecasted_subscribers]                 BIGINT     NOT NULL,
    [total_forecasted_units]                       FLOAT (53) NOT NULL,
    [percentage_of_forecasted_inventory_exposed]   FLOAT (53) NOT NULL,
    [percentage_of_forecasted_inventory_remaining] FLOAT (53) NOT NULL,
    [recommended_eq_hh_cpm]                        MONEY      NULL,
    [minimum_eq_hh_cpm]                            MONEY      NULL,
    CONSTRAINT [PK_proposal_inventory_check_details] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [proposal_detail_id] ASC, [date_created] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_proposal_inventory_check_details_media_months] FOREIGN KEY ([base_media_month_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_proposal_inventory_check_details_proposal_details] FOREIGN KEY ([proposal_detail_id]) REFERENCES [dbo].[proposal_details] ([id]),
    CONSTRAINT [FK_proposal_inventory_check_details_proposal_statuses] FOREIGN KEY ([proposal_status_id]) REFERENCES [dbo].[proposal_statuses] ([id]),
    CONSTRAINT [FK_proposal_inventory_check_details_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);



