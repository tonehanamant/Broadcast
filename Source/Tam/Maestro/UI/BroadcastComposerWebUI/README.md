# React / Redux Boilerplate

## Table of Contents
  1. [Getting Started](#getting-started)
  2. [Scripts](#scripts)
  3. [Features](#features)
      - [Component Library & Style Guide](#component-library--style-guide)
      - [Responsive Grid System](#responsive-grid-system)
      - [Responsive Text System](#responsive-text-system)
      - [Internationalization (i18n)](#internationalization)
      - [Icons](#icons)
      - [Images](#images)
      - [Testing](#testing)
      - [Unit Testing](#unit-testing)
      - [Integration Testing](#integration-testing)
      - [Continuous Integration](#continuous-integration)
      - [Redux Middleware](#redux-middleware)
  4. [Credits](#credits)


## Getting Started
  1. Clone the repo `git clone <SSH|HHTP> one2one-web`
  2. Enter the working directory `cd one2one-web`
  3. Install dependencies `yarn install` (If you don't have yarn installed, you can install it by following the instructions [here](https://yarnpkg.com/lang/en/docs/install/))
  4. Start the solution `yarn start`
  5. A browser window will open with the solution (if it doesn't open, navigate to [http://localhost:3000/](http://localhost:3000))



## Scripts
  - `yarn start` Starts **development** build
  - `yarn preview` Previews the **production** build
  - `yarn build` Builds the **production** build
  - `yarn test` Runs all of the tests
  - `yarn test:auto` Runs all of the test automatically
  - `yarn styleguide` Launches the styleguide
  - `yarn styleguidist:build` Builds the styleguide for distribution
  - `yarn lint:js` Lints `.js && .jsx`
  - `yarn lint:js:fix` Auto fixes `.js && .jsx` errors
  - `yarn lint:js:help` Displays help for eslint CLI


## Features
### Syntax & ES6 (ECMA2015)
ES6 should be used throughout this project at all times. ESLint is configured to recognize deprecated methodologies and will throw an error in the build. All JS techniques are modeled after the [AirBnB React Syntax Style Guide](https://github.com/airbnb/javascript/tree/master/react). ESLint also extends the conventions that AirBnB uses.

Due to the massive update to the JS spec, you can refer to this [ES6 Reference Guide](https://github.com/drewdecarme/es6-reference) here for common concepts such as array and object deconstruction, arrow functions, symbols, and class notation.

### File Structure & Organization
Project structure follows the [Ducks Methodology](https://github.com/erikras/ducks-modular-redux) and was implemented following an article written by [Matteo Mazzarolo](https://hackernoon.com/my-journey-toward-a-maintainable-project-structure-for-react-redux-b05dfd999b5). There are some nuances of the methodology that improve importation clauses and use of variables which are outlined in the [Writing Ducks]. section below.

Here is the project structure 30,000 feet...
```
app/
 |-- components/    (inputs, buttons, links, grid, etc...)
 |-- containers/    (collection of components that access Redux State)
 |-- ducks/         (each containers bundled Redux management)
 |-- sagas/         (TBD)
 |-- services/      (TBD)
```
  1. **Components** - are individual elements that compose great portions of the UI. Components can be but aren't limited to inputs, buttons, grids, etc... pretty much anything that has a standard feel across the entire application. Components can be either presentational (responsive grid system) or stateful (validated inputs)... also known as "dumb and smart" respectively. Each component has an index, style, and test associated with it.
  2. **Containers** - are collection of components that need to interact with the Redux state (a drop down filtering a list of data). Each container has an index, style, and test associated with it.
  3. **Ducks** - is a bundle reducers, action types and actions in the same file, leading to a reduced boilerplate. Each duck aligns with the name of the container. The concept behind ducks is to reduce the work needed to maintain Redux code, while still maintaining the integrity of what Redux represents.
  3. **Sagas** - TBD
  4. **Services** - TBD

Below is a detailed view of the folder structure.
```
app
 |-- components/
 |    |-- component1
 |          |-- index.jsx
 |          |-- index.spec.js
 |          |-- index.style.scss
 |
 |-- containers/
 |    |-- container1
 |    |     |-- index.jsx
 |    |     |-- index.spec.js
 |    |     |-- index.style.scss
 |    |
 |    |-- container2
 |    |-- container3
 |
 |-- ducks
 |    |-- container1.ducks.js
 |    |-- container2.ducks.js
 |    |-- container3.ducks.js
 |
 |-- sagas
 |    |-- container1.saga.js
 |    |-- container2.saga.js
 |    |-- container3.saga.js
 |
 |-- services (TBD)
assets
styles
```

#### Writing Ducks
TBD


### Style Guide
The component library utilizes [React Styleguidist](https://react-styleguidist.js.org/) with the projects custom webpack configuration. React Styleguidist is and isolated React component development environment with a living style guide.

This solution utilizes a concept of a Living Style Guide generated directly from the code of each of the React Components. Outside of a markdown file and some comments in each of the components, there is no other work required to maintain the style guide; a convention which is missing from most projects. [React Styleguidist]() parses the entire component library, the markdown files associated with each component, the comments inside of each of the components, and compiles each `.scss` file imported into the component to create a comprehensive styleguide that can be viewed, shared, and edited in real time.

  - **To create the styleguide** refer to the [Build Scripts](#build-scripts)
  - **To document a component** refer to the [documentation](https://react-styleguidist.js.org/docs/documenting.html)
  - **To configure the layout of the style guide** refer to [configuration documentation](https://react-styleguidist.js.org/docs/configuration.html)

**Todo**
  - [x] Implement Styleguidist
  - [x] Align Styleguidist config to current Webpack development config
  - [ ] Configure Styleguidist Layout and Components to a working state
  - [x] Add ES6 Cheat Sheet Fork Custom ES6 Markdown Cheat Sheet
  - [ ] Review AirBnb react conventions and adjust ESLint file


### Styling Components
Sass is the de-facto standard among most projects now and it's used in the project in conjunction with a derivation of React CSS Modules called [Babel Plugin React CSS Modules](https://github.com/gajus/babel-plugin-react-css-modules). This system allows for the import of a .scss file into the component, reconciles the classes with that of the "styleName" prop, and finally hashes the styles to make sure that they are encapsulated within that component.

Essentially, we're taking the "C" out of "CSS". However, there are global styles that are applied at the react root level that are not hashed (fonts, headers, paragraph tags, etc...)

**SCSS File**
```scss
.testClass{
  display: flex;
  justify-content: center;
  background: blue;
}
```
**Component**
```js
import React, { Component } from 'react';
import './index.style.scss';

export default const Test = () => {
  return (
    <header styleName="testClass">
      <h1>Hello!</h1>
    </header>
  );
};
```


### Responsive Grid System
The [Bootstrap 4 Grid](https://v4-alpha.getbootstrap.com/layout/grid/) is the basis for the grid system in this project. A custom component with props that model the class structure illustrated in the linked documentation was created due to the way that this project binds CSS class imports to components.

Full documentation on the Grid component, how to use it, and what props to pass it can be found in the Style-guide or in the [.readme within the Grid component](). Some of the features for Rows and Columns include but aren't limited to:
  - **Rows**
    - Horizontal Alignment - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#horizontal-alignment)
    - Variable Width Content - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#variable-width-content)
    - Vertical Alignment - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#vertical-alignment)
    - No Gutters - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#no-gutters)
  - **Columns**
    - Offsetting Columns - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#offsetting-columns)
    - Vertical Self Align - [(Docs)](https://v4-alpha.getbootstrap.com/layout/grid/#vertical-alignment)

**Todo**
  - [ ] Create the Grid .readme
  - [ ] Comment the grid component to align with the style guide

### Responsive Text System
[Modularscale Sass](https://github.com/modularscale/modularscale-sass) was added to the base styles of the project to automatically generate an harmonic scale for typography across the application and device sizes.

#### Configuration
The base configuration sets the line height to 1.4 units. Mobile and Desktop base sizes are set to 16px and 18px respectively against a perfect fourth scale (1.2). The scale is respected across breakpoints while the base size changes based upon the response of a tablet sized device.

```sass
$font-line-height: 1.4;
$modularscale: (
  base: 16px,
  ratio: 1.2,
  768px: (
    base: 18px,
    ratio: 1.2,
  )
);
```

**Todo**
 - [ ] add `ms-respond` into the base text sizes

### Internationalization
Internationalization (also known as i18n) is available in this project to be a basis for interpeting locales, dates, plurals etc... across any region needed. [React-intl](https://github.com/yahoo/react-intl) provides various different methodologies that play well with the "react style" to add i18n into any component.

#### Conventions & Quick References
  - Use `<FormattedMessage></FormattedMessage>` when you need the text to have a span in it
  - Use React-Intl API for just plain text
  - `FormattedMessage` id prop should follow the convention `componentName.idLocaltoComponent`;
  - To get more detail on implementation, check out this [step-by-step walk through](https://www.smashingmagazine.com/2017/01/internationalizing-react-apps/)

#### Example
The below example uses both the component based solution as well as the API based solution.
```javascript
import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { FormattedMessage, intlShape, injectIntl, defineMessages } from 'react-intl';

import IconReact from '../../../assets/icons/icon-react.inline.svg';

import './index.scss';

const messages = defineMessages({
  appSubTitle: {
    id: 'header.subtitle',
    defaultMessage: 'React + Redux + Webpack',
    description: 'application subtitle'
  }
});

class Header extends Component {
  constructor(props) {
    super(props);
  }

  render() {
    return (
      <header className="header" styleName="header">
        <IconReact />
        <h1>
          <FormattedMessage
            id="header.title"
            defaultMessage="JS Boilerplate"
            description="title message of the application" />
        </h1>
        <h4>{ this.props.intl.formatMessage(messages.appSubTitle) }</h4>
      </header>
    );
  }
}

Header.propTypes = {
  addTodo: PropTypes.func.isRequired,
  intl: intlShape.isRequired
};

export default injectIntl(Header);
```

**Todo**
  - [ ] Figure out [Babel Plugin React Intl](https://github.com/yahoo/babel-plugin-react-intl)



### Icons
Icons are added to the application using the Webpack 2 [SVG React Loader](https://github.com/jhamlet/svg-react-loader). The loader creates a component after relatively importing. The component can then be used like a normal react component. For more information, visit the [Icon Component .readme]()

```javascript
import IconReact from '../../../assets/icons/icon-react.inline.svg';

import './index.scss';

const Header = () => {
  return (
    <section styleName="reactIcon">
      <IconReact />
    </section>
  )
}
```

**Todo**
 - [ ] Document and Style Guide the Icons
 - [ ] Develop a convention for in-lining dynamically named SVGs
 - [ ] Create Icon Component to use dynamically named SVGs

### Images
Module - [URL Loader](https://github.com/webpack-contrib/url-loader)
  - URL Loader Documentation
  - Component Documentation
  - Create Component

### Testing
In general, unit tests are the most important, followed by integration tests, and then followed by acceptance testing. Test should be kept as close to the testing pyramid as possible.

![Testing Pyramid](https://cdn-images-1.medium.com/max/1600/1*sqEGKTARhKQsLyr6POHajA.gif)

### Unit Testing
#### Testing Tools
Unit Testing is accomplished using the following tools:
  - [Karma](https://karma-runner.github.io/1.0/index.html) - Test runner configured by Webpack 2
  - [Jasmine](https://github.com/jasmine/jasmine) - Behavior-driven development JS testing and assertion framework
  - [PhantomJS](http://phantomjs.org/) - Headless Browser

Karma runs the tests that are developed/written using Jasmine in PhantomJS instead of something such as Chrome or Firefox. Tests should follow the same JS conventions laid out in the AirBnb React development style guide.

#### Writing unit tests
Each unit test should answer 5 questions.
  1. The component I'm testing is the **name** of the test
  2. The component _aspect_ I'm testing is in the **message**
  3. _How_ the component is tested is very clear in the second stanza of the test
  4. The expected result is clear (this is crucial)
  5. The actual result is clear

Expected results should **ALWAYS** be written before the actual result. This helps with driving TDD and should force the engineer to think of what _should_ happen before what does _happen_

#### Structuring of unit tests
Unit tests structure should be broken into 3 parts
  1. Setup
  2. _How_ the test is executed
  3. Expected/actual results

#### Main focus of unit tests
Test should focus solely on input/output of the component; not on the inner implementation of the component. Setup/tests should only reference the _inputs_ and assertions should only check the _outputs_.
  - Input Types
    1. Static Props
    2. Events generated by the user
    3. A child component calling a passed-in function
  - Output Types
    1. The presence of a child component
    2. a call to function prop, like a Redux action creator
    3. Values passed into child component props

#### Adding a new test
Each component & Redux container should contain an `index.test.(js|jsx)` file depending upon if jsx needs to be included within the test.

#### Running the tests
`yarn test` will run all of the tests one time and produce an output
`yarn test:auto` will run the tests and will re-run after any dependent file is changed.

**Todo**
  - [ ] Implement a screen capture function
  - [ ] Add examples for unit tests
  - [ ] (Nice to have) Automate Performance Analysis

### Integration Testing
Integration tests should test where the React application meets the outside REST API or other outside entity. Failing tests should be written first and then successful tests should be written to accommodate the actual results.

### Continuous Integration
[Travis CI](https://travis-ci.org/) is the choice continuous integration tester that is in place

### Redux Middleware
In attempt to manage redux side effects, [redux-saga](https://redux-saga.js.org/docs/introduction/) is used to manage any asynchronous calls to fetch data, manage cache, etc...

### [Normalzr]()

**Todo**
  - [ ] Document redux-saga
  - [ ] add examples


## Credits
#### Javscript
- [React](https://facebook.github.io/react)
- [React Router](https://reacttraining.com/react-router/)
- [Redux](http://redux.js.org/)
- [ES6](http://es6-features.org) (with [Babel](https://babeljs.io))
- [AirBnB React Syntax Style Guide](https://github.com/airbnb/javascript/tree/master/react)
- [ESLint](http://eslint.org/) based upon [AirBnB Best Practices](https://github.com/airbnb/javascript/tree/master/linters)
- [JS Cookie](https://github.com/js-cookie/js-cookie)

#### Style
- [Bootstrap 4 (Grid Only)](https://v4-alpha.getbootstrap.com/)
- [Babel Plugin React CSS Modules](https://github.com/gajus/babel-plugin-react-css-modules) used in conjunction with [Sass](http://sass-lang.com/documentation/file.SCSS_FOR_SASS_USERS.html)
- [Modularscale Sass](https://github.com/modularscale/modularscale-sass)
- [Post CSS](https://github.com/postcss/postcss)
- [Auto Prefixer](https://github.com/postcss/autoprefixer)

#### Asset Pipeline
- [Webpack 2](https://webpack.js.org/) based upon a cool ["how to"](https://blog.madewithenvy.com/getting-started-with-webpack-2-ed2b86c68783)
- [Hot Module Replacement](https://medium.com/@dan_abramov/hot-reloading-in-react-1140438583bf#.xh6v0ht7j) ([React Hot Loader 3](https://github.com/gaearon/react-hot-loader/issues/243))
- [Gulp](http://gulpjs.com/)
- [Browser Sync](https://www.browsersync.io/)
- [Yarn](https://yarnpkg.com/en/)

