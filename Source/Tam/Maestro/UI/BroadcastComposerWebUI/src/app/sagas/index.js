import * as appSaga from './app';
import * as postPrePostingSaga from './postPrePosting';
import * as planningSaga from './planning';

export default function* rootSaga() {
  yield [

    appSaga.watchRequestEnvironment(),
    appSaga.watchRequestEmployee(),
    appSaga.watchReadFileB64(),

    postPrePostingSaga.requestPostPrePostingInitialData(),
    postPrePostingSaga.watchRequestPostPrePosting(),
    postPrePostingSaga.watchRequestAssignPostDisplay(),
    postPrePostingSaga.watchRequestPostPrePostingFiltered(),
    postPrePostingSaga.watchDeletePostPrePostingById(),
    postPrePostingSaga.watchRequestPostPrePostingFileEdit(),
    postPrePostingSaga.watchSavePostPrePostingFileEdit(),
    postPrePostingSaga.watchUploadPostPrePostingFile(),

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

  ];
}
