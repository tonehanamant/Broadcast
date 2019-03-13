import React from "react";
import Enzyme, { shallow, mount /* , render */ } from "enzyme";
import Adapter from "enzyme-adapter-react-16";

/* eslint-disable react/jsx-filename-extension */
/* eslint-disable no-console */

// React 16 Enzyme adapter
Enzyme.configure({
  adapter: new Adapter(),
  disableLifecycleMethods: true
});

// Make Enzyme functions available in all test files without importing
global.shallow = shallow;
global.mount = mount;
// global.render = render;

// helper functions
global.shallowWrapper = Component => (props = {}) =>
  shallow(<Component {...props} />);
global.mountWrapper = Component => props => mount(<Component {...props} />);

// Fail tests on any warning
console.error = message => {
  throw new Error(message);
};
