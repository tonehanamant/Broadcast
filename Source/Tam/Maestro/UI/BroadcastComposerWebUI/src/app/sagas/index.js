import * as appSaga from './app';
import * as postSaga from './post';
import * as planningSaga from './planning';

export default function* rootSaga() {
  yield [

    appSaga.watchRequestEnvironment(),
    appSaga.watchRequestEmployee(),
    appSaga.watchReadFileB64(),

    postSaga.watchRequestPostInitialData(),
    postSaga.watchRequestPost(),
    postSaga.watchRequestAssignPostDisplay(),
    postSaga.watchRequestPostFiltered(),
    postSaga.watchDeletePostById(),
    postSaga.watchRequestPostFileEdit(),
    postSaga.watchSavePostFileEdit(),
    postSaga.watchUploadPostFile(),

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

  ];
}
