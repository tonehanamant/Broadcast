using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.BusinessEngines;

namespace Services.Broadcast.Converters.Scx
{
    /// <summary>
    /// Feature not available
    /// </summary>
    public interface IProposalScxConverter : IApplicationService
    {
        //List<ProposalScxFile> ConvertProposal(ProposalDto proposal);
        //void ConvertProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail, ref ProposalScxFile scxFile);
        //adx BuildFromProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail);
    }
    
    public class ProposalScxConverter : ScxBaseConverter, IProposalScxConverter
    {        
        private readonly IProposalScxDataPrep _proposalScxDataPrep;

        public ProposalScxConverter(IProposalScxDataPrep proposalScxDataPrep
                                    , IDaypartCache daypartCache
                                    , IDateTimeEngine dateTimeEngine) 
            : base(daypartCache, dateTimeEngine)
        {
            _proposalScxDataPrep = proposalScxDataPrep;
        }

        //public List<ProposalScxFile> ConvertProposal(ProposalDto proposal)
        //{
        //    List<ProposalScxFile> scxFiles = new List<ProposalScxFile>();
        //    foreach (var propDetail in proposal.Details)
        //    {
        //        ProposalScxFile scxFile = new ProposalScxFile();
        //        ConvertProposalDetail(proposal, propDetail, ref scxFile);
        //        scxFiles.Add(scxFile);
        //    }

        //    return scxFiles;
        //}

        //public void ConvertProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail, ref ProposalScxFile scxFile)
        //{
        //    adx a = BuildFromProposalDetail(proposal, propDetail);

        //    if (a == null)
        //        return;

        //    string xml = a.Serialize();
        //    var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        //    scxFile = new ProposalScxFile()
        //    {
        //        ProposalDetailDto = propDetail,
        //        ScxStream = stream
        //    };
        //}

        //public adx BuildFromProposalDetail(ProposalDto proposal, ProposalDetailDto propDetail)
        //{
        //    var data = _proposalScxDataPrep.GetDataFromProposalDetail(proposal, propDetail);
        //    if (data.MarketIds.IsNullOrEmpty())
        //        return null;

        //    return CreateAdxObject(data);
        //}
    }
}

