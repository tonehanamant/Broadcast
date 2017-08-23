CREATE TABLE [dbo].[default_company_post_report_options] (
    [company_id]                    INT     NOT NULL,
    [post_buy_analysis_report_code] TINYINT NOT NULL,
    CONSTRAINT [PK_default_company_post_report_options] PRIMARY KEY CLUSTERED ([company_id] ASC, [post_buy_analysis_report_code] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_default_company_post_report_options_companies] FOREIGN KEY ([company_id]) REFERENCES [dbo].[companies] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options', @level2type = N'COLUMN', @level2name = N'company_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options', @level2type = N'COLUMN', @level2name = N'post_buy_analysis_report_code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'default_company_post_report_options', @level2type = N'COLUMN', @level2name = N'post_buy_analysis_report_code';

