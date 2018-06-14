import React, { Fragment } from 'react';
import PropTypes from 'prop-types';
import { ContextMenuProvider, Item, ContextMenu } from 'react-contexify';
import { map } from 'lodash/fp';

import 'react-contexify/dist/ReactContexify.min.css';
import './index.scss';

const onClick = ({ data }) => {
  const { EVENT_HANDLER, row } = data;
  EVENT_HANDLER({ metaData: { rowData: row.toJS() } });
};

const generateMenuItems = row => map(({ key, text, EVENT_HANDLER }) => (
  <Item key={key} onClick={onClick} data={{ EVENT_HANDLER, row }}>{text}</Item>
));

const ContextMenuRow = (props) => {
  const { rowProps, row, menuItems, isRender, stateKey } = props;

  if (!isRender && !menuItems) {
    return (
      <tr {...rowProps}>
        {props.children}
      </tr>
    );
  }

  const rowId = row.get('_key');
  const menuId = `${stateKey}-${rowId}`;
  return (
    <Fragment>
      <ContextMenuProvider
        id={menuId}
        storeRef={false}
        render={({ children, ...rest }) => (
          <tr
            {...rowProps}
            {...rest}
            className={rowProps.className}
          >
            {children}
          </tr>
        )}
      >
          { props.children }
      </ContextMenuProvider>
      <ContextMenu id={menuId}>
        { generateMenuItems(row)(menuItems) }
      </ContextMenu>
    </Fragment>
  );
};

ContextMenuRow.defaultProps = {
  isRender: true,
  menuItems: undefined,
};

ContextMenuRow.propTypes = {
  children: PropTypes.oneOfType([PropTypes.arrayOf(PropTypes.element), PropTypes.element]).isRequired,
  menuItems: PropTypes.arrayOf(PropTypes.object),
  rowProps: PropTypes.any.isRequired,
  row: PropTypes.any.isRequired,
  stateKey: PropTypes.string.isRequired,
  isRender: PropTypes.bool,
};

export default ContextMenuRow;

