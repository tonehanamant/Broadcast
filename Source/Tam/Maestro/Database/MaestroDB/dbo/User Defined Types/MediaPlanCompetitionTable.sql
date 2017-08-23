CREATE TYPE [dbo].[MediaPlanCompetitionTable] AS TABLE (
    [proposal_id]               INT          NOT NULL,
    [unique_proposal_detail_id] VARCHAR (63) NOT NULL,
    [media_month_id]            SMALLINT     NOT NULL,
    [media_week_id]             INT          NOT NULL,
    [network_id]                INT          NOT NULL,
    [component_daypart_id]      INT          NOT NULL,
    [proposal_status_id]        TINYINT      NOT NULL,
    [hh_eq_cpm_start]           MONEY        NOT NULL,
    [hh_eq_cpm_end]             MONEY        NOT NULL,
    [subscribers]               BIGINT       NOT NULL,
    PRIMARY KEY CLUSTERED ([proposal_id] ASC, [unique_proposal_detail_id] ASC, [media_month_id] ASC, [media_week_id] ASC, [network_id] ASC, [component_daypart_id] ASC, [hh_eq_cpm_start] ASC, [hh_eq_cpm_end] ASC));

