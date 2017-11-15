import React from 'react';
import { Route } from 'react-router-dom';

import AppMain from 'Containers/AppMain';

const AppRoot = () => (
    <Route path="/test" component={AppMain} />
);

export default AppRoot;
