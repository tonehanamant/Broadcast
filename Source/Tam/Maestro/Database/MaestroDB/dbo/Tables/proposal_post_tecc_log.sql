CREATE TABLE [dbo].[proposal_post_tecc_log] (
    [proposal_id]      INT      NOT NULL,
    [tam_post_id]      INT      NOT NULL,
    [employee_id]      INT      NOT NULL,
    [transmitted_date] DATETIME CONSTRAINT [DF_proposal_post_tecc_log_transmitted_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_proposal_post_tecc_log] PRIMARY KEY CLUSTERED ([proposal_id] ASC, [tam_post_id] ASC, [employee_id] ASC, [transmitted_date] ASC),
    CONSTRAINT [FK_proposal_post_tecc_log_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_proposal_post_tecc_log_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_proposal_post_tecc_log_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);

