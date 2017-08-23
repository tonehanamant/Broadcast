
CREATE PROCEDURE [dbo].[usp_ACCT_GetTelamericaInvoiceHistoryForDisplayByDate]
      @from_month int
      ,@from_year int 
      ,@to_month int
      ,@to_year int
      ,@effective_date datetime
AS
BEGIN
      SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
      SET NOCOUNT ON;
      declare @from_date smalldatetime
      declare @to_date smalldatetime
      
      set @from_date = convert(smalldatetime, convert(varchar(2), @from_month) + '/1/' + convert(varchar(4), @from_year))
      set @to_date = convert(smalldatetime, convert(varchar(2), @to_month) + '/1/' + convert(varchar(4), @to_year))
      
      create table #media_months (id int)

      insert into #media_months
      Select id
      from media_months mm  WITH (NOLOCK)
      where convert(smalldatetime, convert(varchar(2), mm.month) + '/1/' + convert(varchar(4), mm.year)) 
            between @from_date and @to_date
            
      SELECT 
         ri.receivable_invoice_id
        ,ri.start_date
        ,ri.end_date
      ,ri.media_month_id
      ,ri.entity_id
      ,product_id = pr.id
      ,p.advertiser_company_id
        ,p.agency_company_id
        ,product = pr.name
        ,proposal_name = p.name
      ,ri.customer_number
      ,gp.customer_name
      ,ri.invoice_number
      ,ri.special_notes
      ,ri.total_units
      ,ri.total_due_gross
      ,ri.total_due_net
      ,ri.total_credits
      ,ri.document_id
      ,ri.is_mailed
      ,ri.ISCI_codes
      ,ri.active
      ,ri.date_created
      ,ri.date_modified
      ,ri.modified_by
      ,billing_terms_id = bt.id
      ,billing_terms = bt.name
      ,salesperson_firstname = e.firstname
      ,salesperson_lastname = e.lastname
      ,default_spot_length = sl.length
      ,p.finance_notes
      ,d.daypart_text
      ,p.is_invoice_weekly_gross_split_required
      ,p.are_iscis_required_on_invoice
      ,p.billing_flow_chart_code
  FROM uvw_receivable_invoice_universe ri 
  JOIN proposals p  ON p.id=ri.entity_id
  JOIN great_plains_customers gp  on ri.customer_number = gp.customer_number
  JOIN dbo.spot_lengths sl ON sl.id = p.default_spot_length_id
  JOIN dbo.dayparts d ON d.id = p.primary_daypart_id
  LEFT JOIN billing_terms bt ON bt.id = p.billing_terms_id
  LEFT JOIN dbo.employees e ON e.id = p.salesperson_employee_id
  LEFT JOIN products pr  ON pr.id=p.product_id 
  WHERE exists
      (SELECT 1
            FROM #media_months mm
            WHERE ri.media_month_id = mm.id
      )
  AND (ri.start_date >= @effective_date
            AND
            (ri.end_date <= @effective_date
            OR ri.end_date IS NULL))
  AND ri.invoice_type_id = 1
  ORDER BY ri.invoice_number
  
 END
