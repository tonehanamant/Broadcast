CREATE TABLE [dbo].[tam_post_proposals] (
    [id]                         INT        IDENTITY (1, 1) NOT NULL,
    [tam_post_id]                INT        NOT NULL,
    [posting_plan_proposal_id]   INT        NOT NULL,
    [post_source_code]           TINYINT    NOT NULL,
    [total_spots_in_spec]        INT        NOT NULL,
    [total_spots_out_of_spec]    INT        NOT NULL,
    [posted_by_employee_id]      INT        NULL,
    [post_duration]              FLOAT (53) NULL,
    [post_started]               DATETIME   NULL,
    [post_completed]             DATETIME   NULL,
    [aggregation_status_code]    TINYINT    CONSTRAINT [DF_tam_post_proposals_aggregated] DEFAULT ((0)) NOT NULL,
    [aggregation_duration]       FLOAT (53) NULL,
    [aggregation_started]        DATETIME   NULL,
    [aggregation_completed]      DATETIME   NULL,
    [number_of_zones_delivering] INT        NOT NULL,
    [date_exported_to_msa]       DATETIME   NULL,
    [msa_status_code]            TINYINT    NOT NULL,
    CONSTRAINT [PK_tam_post_proposals] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_proposals_employees] FOREIGN KEY ([posted_by_employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_tam_post_proposals_proposals] FOREIGN KEY ([posting_plan_proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_tam_post_proposals_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tam_post_proposals]
    ON [dbo].[tam_post_proposals]([tam_post_id] ASC, [posting_plan_proposal_id] ASC, [post_source_code] ASC)
    INCLUDE([id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains all the posting plans associated with a post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Full Name', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'TAM Post Proposals', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Part of Production', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the TAM post record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'tam_post_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the posting plan record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'posting_plan_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'posting_plan_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Enum', @value = N'EPostSourceCode', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_source_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total number of spots posted to this proposal for this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'total_spots_in_spec';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'total_spots_in_spec';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The total number of spots that were out of spec to this proposal for this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'total_spots_out_of_spec';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'The spots that were out of spec are stored in the posted_affidavits_out_of_spec table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'total_spots_out_of_spec';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Employee who last posted this posting plan in this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'posted_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'posted_by_employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total amount of time (in seconds) its taken to post and aggregate this proposal for this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Start datetime of the posting process.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_started';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_started';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'End datetime of the posting process.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'post_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The status of the aggregation for this posting plan in this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'0=Awaiting Aggregation, 1=Aggregated', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_status_code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Total time (in seconds) its taken to aggregate this posting plan for this post.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_duration';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Start datetime of the aggregation process.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_started';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_started';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'End datetime of the aggregation process.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'aggregation_completed';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'number_of_zones_delivering';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_proposals', @level2type = N'COLUMN', @level2name = N'number_of_zones_delivering';

