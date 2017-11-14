import React from 'react';
import { Route } from 'react-router-dom';

import AppMain from 'Containers/AppMain';

const AppRoot = () => (
    <Route path="/broadcast" component={AppMain} />
);

export default AppRoot;
