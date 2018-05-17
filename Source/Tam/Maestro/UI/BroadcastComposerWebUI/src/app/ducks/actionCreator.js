export const REQUEST = 'REQUEST';
export const SUCCESS = 'SUCCESS';
export const FAILURE = 'FAILURE';

export const createAction = action => ({
    request: `${action}/${REQUEST}`,
    success: `${action}/${SUCCESS}`,
    failure: `${action}/${FAILURE}`,
});

