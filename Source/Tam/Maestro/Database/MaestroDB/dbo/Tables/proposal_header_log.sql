CREATE TABLE [dbo].[proposal_header_log] (
    [proposal_id]   INT      NOT NULL,
    [employee_id]   INT      NOT NULL,
    [uploaded_date] DATETIME CONSTRAINT [DF_proposal_header_log_uploaded_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_proposal_header_log] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [employee_id] ASC, [uploaded_date] ASC),
    CONSTRAINT [FK_proposal_header_log_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposal_header_log_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id])
);

