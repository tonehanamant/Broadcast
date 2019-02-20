import * as planningSaga from "./planning";

export default function* rootSaga() {
  yield [
    planningSaga.watchRequestProposalInitialData(),
    planningSaga.watchRequestProposals(),
    planningSaga.watchRequestProposalLock(),
    planningSaga.watchRequestProposalUnlock(),
    planningSaga.watchRequestProposal(),
    planningSaga.watchRequestProposalVersions(),
    planningSaga.watchRequestProposalVersion(),
    planningSaga.watchSaveProposal(),
    planningSaga.watchSaveProposalAsVersion(),
    planningSaga.watchDeleteProposalById(),
    planningSaga.watchUpdateProposal(),
    planningSaga.watchModelNewProposalDetail(),
    planningSaga.watchModelUnorderProposal(),
    planningSaga.watchRequestGenres(),
    planningSaga.watchRequestPrograms(),
    planningSaga.watchRequestShowTypes(),
    planningSaga.watchDeleteProposalDetail(),
    planningSaga.watchRequestPlanningFiltered(),
    planningSaga.watchRerunPostScrubing(),
    planningSaga.watchLoadOpenMarketData(),
    planningSaga.watchUpdateEditMarketsData(),
    planningSaga.watchUpdateEditMarketsDataSuccess(),
    planningSaga.watchUpdateProprietaryCpms(),
    planningSaga.watchUploadSCXFile(),
    planningSaga.watchAllocateSpots(),
    planningSaga.watchFilterOpenMarketData(),
    planningSaga.watchLoadPricingDataSuccess(),
    planningSaga.watchLoadPricingData(),
    planningSaga.watchSavePricingData(),
    planningSaga.watchSavePricingDataSuccess(),
    planningSaga.watchCopyToBuyFlow(),
    planningSaga.watchCopyToBuySaga(),
    planningSaga.watchGenerateScx(),
    planningSaga.watchGenerateScxSuccess()
  ];
}
