CREATE TABLE [dbo].[broadcast_proposal_details] (
    [id]                                    INT            IDENTITY (1, 1) NOT NULL,
    [broadcast_proposal_id]                 INT            NOT NULL,
    [original_broadcast_proposal_detail_id] INT            NULL,
    [revision]                              INT            NOT NULL,
    [budget]                                MONEY          NOT NULL,
    [impressions]                           INT            NOT NULL,
    [vig]                                   FLOAT (53)     NOT NULL,
    [spot_length_id]                        INT            NOT NULL,
    [broadcast_daypart_id]                  INT            NOT NULL,
    [notes]                                 VARCHAR (2047) NULL,
    [employee_id]                           INT            NULL,
    [separation]                            SMALLINT       NULL,
    [proposal_detail_status_id]             TINYINT        NOT NULL,
    CONSTRAINT [PK_broadcast_proposal_details] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_broadcast_proposal_detail_statuses_statusid] FOREIGN KEY ([proposal_detail_status_id]) REFERENCES [dbo].[broadcast_proposal_detail_statuses] ([id]),
    CONSTRAINT [FK_broadcast_proposal_details_broadcast_proposals] FOREIGN KEY ([broadcast_proposal_id]) REFERENCES [dbo].[broadcast_proposals] ([id]),
    CONSTRAINT [FK_broadcast_proposal_details_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [unq_broadcast_proposal_details]
    ON [dbo].[broadcast_proposal_details]([original_broadcast_proposal_detail_id] ASC, [revision] ASC) WITH (FILLFACTOR = 90, ALLOW_PAGE_LOCKS = OFF);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'broadcast_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'original_broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'original_broadcast_proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'revision';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'revision';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'budget';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'budget';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'impressions';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'impressions';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'vig';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'vig';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'spot_length_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'broadcast_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'broadcast_daypart_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'notes';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'notes';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'employee_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'separation';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'separation';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'proposal_detail_status_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'broadcast_proposal_details', @level2type = N'COLUMN', @level2name = N'proposal_detail_status_id';

