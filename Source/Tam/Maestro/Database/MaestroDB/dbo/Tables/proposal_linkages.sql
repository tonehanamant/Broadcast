CREATE TABLE [dbo].[proposal_linkages] (
    [proposal_linkage_type] TINYINT  NOT NULL,
    [primary_proposal_id]   INT      NOT NULL,
    [linked_proposal_id]    INT      NOT NULL,
    [date_created]          DATETIME NOT NULL,
    CONSTRAINT [PK_proposal_linkages] PRIMARY KEY CLUSTERED ([proposal_linkage_type] ASC, [primary_proposal_id] ASC, [linked_proposal_id] ASC),
    CONSTRAINT [FK_proposal_linkages_proposals] FOREIGN KEY ([primary_proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_proposal_linkages_proposals1] FOREIGN KEY ([linked_proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

