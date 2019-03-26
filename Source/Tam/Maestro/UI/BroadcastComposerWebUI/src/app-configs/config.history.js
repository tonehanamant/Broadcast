import createHistory from "history/createBrowserHistory";

// DO NOT CHANGE THIS VARIABLE
// this is a variable that is set internally with the configuration
// mechanism. Changing this variable will cause the application to break
// eslint-disable-next-line no-undef
export const history = createHistory({ basename: __CADENT_PUBLIC_PATH__ });
