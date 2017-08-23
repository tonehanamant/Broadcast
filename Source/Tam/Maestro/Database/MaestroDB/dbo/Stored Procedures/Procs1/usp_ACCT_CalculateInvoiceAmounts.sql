/* END employees refactor */

/* products Refactor 2 (same procedure as some previous sections */
CREATE PROCEDURE [dbo].[usp_ACCT_CalculateInvoiceAmounts]
      @year INT,
      @month INT
AS
BEGIN
      SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
      declare @max_invoice_num int
      declare @media_month_id int 
      
      Select @media_month_id = id
      from media_months WITH (NOLOCK)
      where year = @year
      and month = @month
      
      Select @max_invoice_num = isnull(max(cast(substring(invoice_number, 5, 4) as int)), 0)
      from receivable_invoices ri  WITH (NOLOCK)
      where ri.media_month_id = @media_month_id
      and invoice_type_id = 1 -- TelAmerica Invoices
      
      SELECT
            InvoiceNum =  
                  + right('0' + convert(varchar, @month), 2)
                  + right('00' + substring(convert(varchar, @year), 3, 2), 2)
                  + right('000' + convert(varchar, @max_invoice_num + ROW_NUMBER() OVER (Order by CURRENT_TIMESTAMP) ), 4)
            ,Advertiser = ISNULL(map.advertiser_alias, adv.name)
            ,Agency = agy.name
            ,ProductAlias = map.product_alias
            ,ProposalName = p.name
            ,ProductId = p.product_id
            ,MediaMonthId = mm.id
            ,ProposalId = pd.proposal_id
            ,Units = SUM(pdw.units)
            ,GrossAmt = SUM(pdw.units * pd.proposal_rate)
            ,NetAmt = ROUND(SUM(pdw.units * pd.proposal_rate) * 0.85 , 2)
            ,CreditAmt = 0.0
            ,CustomerNumber = cust.customer_number
            ,CustomerName = cust.customer_name
            ,SalesRepId = p.salesperson_employee_id
            ,RequiresISCI = cust.include_ISCI
            ,BillingTerms = bt.name
            ,BillingTermsId = bt.id
            ,DefaultSpotLength = sl.length
	    ,p.finance_notes
	    ,d.daypart_text
	    ,p.is_invoice_weekly_gross_split_required
	    ,p.are_iscis_required_on_invoice
	    ,p.billing_flow_chart_code
      FROM
            proposal_detail_worksheets pdw 
            JOIN proposal_details pd  ON pd.id=pdw.proposal_detail_id
            JOIN proposals p  ON p.id=pd.proposal_id
            JOIN media_weeks mw  ON mw.id=pdw.media_week_id
            JOIN media_months mm  ON mm.id=mw.media_month_id
            JOIN dbo.spot_lengths sl ON sl.id = p.default_spot_length_id
            JOIN dbo.dayparts d ON d.id = p.primary_daypart_id
            LEFT JOIN billing_terms bt ON bt.id = p.billing_terms_id
            LEFT JOIN companies adv  ON adv.id=p.advertiser_company_id
            LEFT JOIN companies agy  ON agy.id=p.agency_company_id
            LEFT JOIN great_plains_mapping map  ON map.product_id = p.product_id
            LEFT JOIN great_plains_customers cust  ON cust.customer_number = map.great_plains_customer_number
      WHERE
            p.proposal_status_id=4
            AND mm.id = @media_month_id
      GROUP BY
            agy.name
            ,ISNULL(map.advertiser_alias, adv.name)
            ,map.product_alias
            ,p.product_id
            ,p.name
            ,mm.id
            ,pd.proposal_id
            ,cust.customer_number
            ,cust.customer_name
            ,p.salesperson_employee_id
            ,cust.include_ISCI
            ,bt.name
            ,bt.id
			,sl.length
			,p.finance_notes
		    ,d.daypart_text
		    ,p.is_invoice_weekly_gross_split_required
		    ,p.are_iscis_required_on_invoice
		    ,p.billing_flow_chart_code
END


