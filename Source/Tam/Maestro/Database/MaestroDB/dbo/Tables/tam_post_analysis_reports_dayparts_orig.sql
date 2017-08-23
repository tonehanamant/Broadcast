CREATE TABLE [dbo].[tam_post_analysis_reports_dayparts_orig] (
    [tam_post_proposal_id] INT        NOT NULL,
    [audience_id]          INT        NOT NULL,
    [network_id]           INT        NOT NULL,
    [daypart_id]           INT        NOT NULL,
    [enabled]              BIT        NOT NULL,
    [subscribers]          BIGINT     NOT NULL,
    [delivery]             FLOAT (53) NOT NULL,
    [eq_delivery]          FLOAT (53) NOT NULL,
    [units]                FLOAT (53) NOT NULL,
    [dr_delivery]          FLOAT (53) NOT NULL,
    [dr_eq_delivery]       FLOAT (53) NOT NULL,
    [total_spots]          INT        NULL
);

