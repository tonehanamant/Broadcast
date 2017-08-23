CREATE TABLE [dbo].[proposal_settings] (
    [proposal_id]                           INT NOT NULL,
    [hide_proposal_in_traffic_planning_app] BIT NOT NULL,
    CONSTRAINT [PK_proposal_settings] PRIMARY KEY CLUSTERED ([proposal_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_proposal_settings_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id]) ON DELETE CASCADE
);

