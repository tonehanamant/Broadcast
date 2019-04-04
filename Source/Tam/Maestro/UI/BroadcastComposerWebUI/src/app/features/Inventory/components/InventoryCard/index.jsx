import React, { Component } from "react";
import PropTypes from "prop-types";
import { Link } from "react-router-dom";
import CSSModules from "react-css-modules";
import moment from "moment";

import Icon from "Patterns/Icon";
import ImagePlaceholder from "Patterns/ImagePlaceholder";

import CardMenu from "./CardMenu";
import CardField from "./CardField";

import styles from "./index.style.scss";

class InventoryCard extends Component {
  constructor(props) {
    super(props);
    this.state = { isMenuOpen: false };
    this.handleMenuToggle = this.handleMenuToggle.bind(this);
  }

  handleMenuToggle() {
    const { isMenuOpen } = this.state;

    this.setState({
      isMenuOpen: !isMenuOpen
    });
  }

  render() {
    const {
      menuOptions,
      goTo,
      title,
      logo,
      updatedTime,
      markets,
      stations,
      units,
      dayparts,
      impression,
      quarter,
      rates
    } = this.props;
    const { isMenuOpen } = this.state;

    return (
      <li styleName="card-container respond" className="animated slideInUp">
        <div styleName="card">
          {menuOptions.length > 0 && isMenuOpen && (
            <CardMenu
              menuOptions={menuOptions}
              toggleMenu={this.handleMenuToggle}
            />
          )}
          <Link to={goTo}>
            <div styleName="card-logo">
              {logo ? <img src={logo} alt={title} /> : <ImagePlaceholder />}
            </div>
          </Link>
          <div styleName="card-info">
            <div styleName="card-info-row">
              <span styleName="card-info-title">{title}</span>
              <span styleName="card-info-daypart">{quarter}</span>
            </div>
            <div styleName="card-info-row">
              <CardField label="Markets" value={markets} />
              <CardField label="Stations" value={stations} />
              <CardField label="Units" value={units} />
              <CardField label="DayParts" value={dayparts} />
            </div>
            <div styleName="card-info-row">
              <CardField
                label="HH Imp.(000)"
                value={impression}
                description="impression"
              />
              <CardField label="Rates Available" value={rates} />
            </div>
          </div>
          <div styleName="card-footer">
            <span>Updated {moment(updatedTime).fromNow()}</span>
            <button
              type="button"
              styleName="card-sub-menu"
              onClick={this.handleMenuToggle}
            >
              <Icon
                icon="ellipsis-h"
                iconType="light"
                iconAriaLabel="click for more options"
                iconSize="sm"
                iconColor="gray-3"
              />
            </button>
          </div>
        </div>
      </li>
    );
  }
}

InventoryCard.propTypes = {
  menuOptions: PropTypes.arrayOf(
    PropTypes.shape({
      label: PropTypes.string,
      action: PropTypes.func
    })
  ),
  title: PropTypes.string.isRequired,
  logo: PropTypes.string,
  updatedTime: PropTypes.string,
  goTo: PropTypes.string,
  markets: PropTypes.number,
  stations: PropTypes.number,
  units: PropTypes.number,
  dayparts: PropTypes.number,
  impression: PropTypes.number,
  rates: PropTypes.string,
  quarter: PropTypes.string
};

InventoryCard.defaultProps = {
  menuOptions: [],
  logo: null,
  goTo: "#",
  updatedTime: null,
  markets: 0,
  stations: 0,
  units: 0,
  dayparts: 0,
  impression: 0,
  quarter: null,
  rates: null
};

const CardInventoryStyled = CSSModules(InventoryCard, styles, {
  allowMultiple: true
});

export default CardInventoryStyled;
