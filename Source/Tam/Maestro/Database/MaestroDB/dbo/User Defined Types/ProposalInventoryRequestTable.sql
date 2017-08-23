CREATE TYPE [dbo].[ProposalInventoryRequestTable] AS TABLE (
    [unique_proposal_detail_id]       VARCHAR (63) NOT NULL,
    [media_month_id]                  SMALLINT     NOT NULL,
    [media_week_id]                   INT          NOT NULL,
    [network_id]                      INT          NOT NULL,
    [daypart_id]                      INT          NOT NULL,
    [hh_eq_cpm]                       MONEY        NOT NULL,
    [contracted_hh_coverage_universe] INT          NOT NULL,
    [contracted_units]                INT          NOT NULL,
    [contracted_subscribers]          BIGINT       NOT NULL,
    PRIMARY KEY CLUSTERED ([unique_proposal_detail_id] ASC, [media_month_id] ASC, [media_week_id] ASC, [network_id] ASC, [daypart_id] ASC, [hh_eq_cpm] ASC));

