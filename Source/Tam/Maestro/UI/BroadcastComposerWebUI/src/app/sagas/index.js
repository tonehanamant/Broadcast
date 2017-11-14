import * as appSaga from './app';
import * as postSaga from './post';

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

  ];
}
