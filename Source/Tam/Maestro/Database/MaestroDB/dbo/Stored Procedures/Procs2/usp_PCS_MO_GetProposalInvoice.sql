CREATE PROCEDURE usp_PCS_MO_GetProposalInvoice    
(    
 @proposal_id int,    
 @invoice_number int    
)    
AS     
SELECT     
 ri.total_due_gross,     
 ri.total_due_net,     
 ri.total_credits,     
 ri.date_created,     
 ri.invoice_number,     
 bt.code,    
 case when p.original_proposal_id is null then p.id else p.original_proposal_id end  ,  
 ri.id  
FROM     
 receivable_invoices (NOLOCK) ri    
 join proposals p (NOLOCK) on ri.entity_id = p.id    
 join billing_terms bt (NOLOCK) on bt.id = p.billing_terms_id    
WHERE    
 ri.entity_id = @proposal_id AND ri.invoice_number = @invoice_number; 
 
