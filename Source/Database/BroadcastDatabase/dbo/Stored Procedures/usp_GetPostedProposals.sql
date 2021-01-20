/*************************************** END PRI-5655 *****************************************************/

/*************************************** START PRI-7081 *****************************************************/
CREATE PROCEDURE [dbo].[usp_GetPostedProposals]
AS
BEGIN

	SELECT
		v.proposal_id as ContractId ,
		v.equivalized as Equivalized,
		p.Name as ContractName,
		(select Max(af.created_date) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				join affidavit_file_details afd on s.affidavit_file_detail_id = afd.id
				join affidavit_files af on af.id = afd.affidavit_file_id
				where d.proposal_version_id = v.id) as UploadDate ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 2) as SpotsInSpec ,
		(select count(s.id) 
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				join proposal_version_detail_quarter_weeks w on w.proposal_version_quarter_id = q.id 
				join affidavit_client_scrubs s on s.proposal_version_detail_quarter_week_id = w.id
				where d.proposal_version_id = v.id and s.status = 1)as SpotsOutOfSpec ,
		p.advertiser_id as AdvertiserId,
		v.guaranteed_audience_id as GuaranteedAudienceId,
		v.post_type as PostType,
     		(select isnull(sum(q.impressions_goal),0)  --Added isnull 2019-04-08
			from proposal_version_details d join proposal_version_detail_quarters q on d.id = q.proposal_version_detail_id
				where d.proposal_version_id = v.id) as PrimaryAudienceBookedImpressions 
	from proposal_versions v
		join proposals p on p.id = v.proposal_id
	where v.status = 3
END