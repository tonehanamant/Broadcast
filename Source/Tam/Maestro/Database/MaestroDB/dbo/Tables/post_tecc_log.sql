CREATE TABLE [dbo].[post_tecc_log] (
    [tam_post_id]      INT      NOT NULL,
    [result_status]    INT      NOT NULL,
    [employee_id]      INT      NOT NULL,
    [transmitted_date] DATETIME CONSTRAINT [DF_post_tecc_log_transmitted_date] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_post_tecc_log] PRIMARY KEY CLUSTERED ([tam_post_id] ASC, [employee_id] ASC, [transmitted_date] ASC),
    CONSTRAINT [FK_post_tecc_log_employees] FOREIGN KEY ([employee_id]) REFERENCES [dbo].[employees] ([id]),
    CONSTRAINT [FK_post_tecc_log_tam_posts] FOREIGN KEY ([tam_post_id]) REFERENCES [dbo].[tam_posts] ([id])
);

