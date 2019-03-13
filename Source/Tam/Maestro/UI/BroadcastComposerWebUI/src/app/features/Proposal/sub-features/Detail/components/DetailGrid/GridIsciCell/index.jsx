import React, { Component } from "react";
import PropTypes from "prop-types";
import _ from "lodash";
import {
  Button,
  ButtonToolbar,
  Glyphicon,
  Popover,
  Tooltip,
  OverlayTrigger,
  FormGroup,
  FormControl,
  ControlLabel,
  HelpBlock
} from "react-bootstrap";

const getValidationWarnings = iscisData => {
  const ret = [];
  iscisData.forEach((isci, idx) => {
    const inner = [`Error line ${idx + 1}: `];
    let lineValid = true;
    if (isci.HouseIsci === null) {
      lineValid = false;
      inner.push("House ISCI cannot be empty; ");
    }
    if (isci.ClientIsci === null) {
      lineValid = false;
      inner.push("Client ISCI cannot be empty; ");
    }
    if (isci.Days === null) {
      lineValid = false;
      inner.push("Day Include cannot be empty; ");
    }
    if (isci.dayError) {
      lineValid = false;
      inner.push(
        "Day Include invalid days or not unique (M, T, W, Th, F, Sa, Su); "
      );
    }
    if (isci.errorLength) {
      lineValid = false;
      inner.push("Too many values entered (limit 4);");
    }
    if (!lineValid) {
      const joinedInner = inner.join(" ");
      const key = `isci_error_${idx + 1}`;
      ret.push(<div key={key}>{joinedInner}</div>);
    }
  });
  return ret;
};

const checkDaysIncludeValid = days => {
  const daysSplit = days.split("-");
  let check = daysSplit.every(day => {
    const dayval = day.toLowerCase();
    return _.includes(["m", "t", "w", "th", "f", "sa", "su"], dayval);
  });
  if (check) {
    check = _.uniq(daysSplit).length === daysSplit.length;
  }
  return check;
};

export default class GridIsciCell extends Component {
  constructor(props) {
    super(props);
    this.state = {
      iscisDisplay: "",
      iscisValue: "",
      isEdit: false,
      isChanged: false,
      isValid: null,
      isInitiallyValid: true,
      validationErrors: ""
    };
    this.onChangeIscis = this.onChangeIscis.bind(this);
    this.readIscisFromData = this.readIscisFromData.bind(this);
    this.writeIscisFromValues = this.writeIscisFromValues.bind(this);
    this.onSaveIscis = this.onSaveIscis.bind(this);
    this.onSaveIscisNext = this.onSaveIscisNext.bind(this);
    this.saveIscis = this.saveIscis.bind(this);
    this.popover = null;
    this.closePopover = this.closePopover.bind(this);
    this.showPopover = this.showPopover.bind(this);
  }

  componentWillReceiveProps(nextProps) {
    this.setState({ validationErrors: "" });
    this.setState({ isValid: null });
    this.readIscisFromData(nextProps.Iscis);
  }

  // change - set changed
  onChangeIscis(event) {
    const val = event.target.value;
    this.setState({ iscisValue: val, isChanged: true });
  }

  onSaveIscis() {
    this.saveIscis(false);
  }

  // next - send the week count + 1 - detail grid will open as needed
  onSaveIscisNext() {
    const { hasNext, weekCnt } = this.props;
    if (hasNext) {
      const next = weekCnt + 1;
      this.saveIscis(next);
    }
  }

  writeIscisFromValues(iscisValue) {
    const { isChanged, isInitiallyValid } = this.state;
    const { Iscis } = this.props;
    if (isChanged || !isInitiallyValid) {
      let valid = true;
      let iscisData = [];
      if (iscisValue && iscisValue.match(/([^\r\n]+)/g)) {
        iscisData = iscisValue.match(/([^\r\n]+)/g).map(entry => {
          const splitted = entry.split(",");
          valid = valid
            ? splitted[0] && splitted[1] && splitted[2] && !splitted[4]
            : false;
          // console.log('isci check', splitted, valid);
          let house = splitted[0] ? splitted[0].trim() : null;
          let married = false;
          if (house) {
            married = house.indexOf("(m)") !== -1;
            if (married) house = house.replace("(m)", "");
          }
          // check for valid days
          let dayCheck = true;
          if (splitted[2]) {
            dayCheck = checkDaysIncludeValid(splitted[2].trim());
            if (!dayCheck) valid = false;
          }
          const ret = {
            HouseIsci: house,
            ClientIsci: splitted[1] ? splitted[1].trim() : null,
            Days: splitted[2] ? splitted[2].trim() : null,
            Brand: splitted[3] ? splitted[3].trim() : null,
            MarriedHouseIsci: married,
            dayError: !dayCheck,
            errorLength: splitted[4] !== undefined
            // isValid: valid,
          };
          return ret;
        });
      }
      return { isValid: valid, data: iscisData };
    }
    return { isValid: true, data: Iscis };
  }

  saveIscis(next) {
    const { saveInputIscis } = this.props;
    const { iscisValue } = this.state;
    const checkIscis = this.writeIscisFromValues(iscisValue);
    if (checkIscis.isValid) {
      this.closePopover();
      saveInputIscis(checkIscis.data, next);
    } else {
      const errors = getValidationWarnings(checkIscis.data);
      this.setState({ validationErrors: errors });
      this.setState({ isValid: "error" });
    }
  }

  // display/edit conversion - both for popover and edit house display/tips
  readIscisFromData(iscis) {
    if (iscis && iscis.length) {
      const houseDisplay = [];
      let isInitiallyValid = true;
      const iscisFormatted = iscis.reduce((aggregated, iscisItem) => {
        const house = iscisItem.HouseIsci || "";
        const client = iscisItem.ClientIsci || "";
        const days = iscisItem.Days || "";
        const brand = iscisItem.Brand || "";
        const married = iscisItem.MarriedHouseIsci ? "(m)" : "";
        houseDisplay.push(house.trim() + married);
        if (isInitiallyValid) {
          isInitiallyValid =
            iscisItem.HouseIsci !== null &&
            iscisItem.ClientIsci !== null &&
            iscisItem.Days !== null;
        }
        return `${aggregated +
          house.trim()}${married},${client.trim()},${days.trim()},${brand.trim()}\n`;
      }, "");
      // console.log('read iscis >>>', isInitiallyValid);

      this.setState({
        iscisValue: iscisFormatted,
        iscisDisplay: houseDisplay,
        isInitiallyValid,
        isEdit: true
      });
    } else {
      this.setState({
        iscisValue: "",
        iscisDisplay: "",
        isInitiallyValid: true,
        isEdit: false
      });
    }
  }

  closePopover() {
    this.popover.hide();
  }

  showPopover() {
    this.popover.show();
  }

  render() {
    const {
      isEdit,
      isValid,
      iscisValue,
      validationErrors,
      iscisDisplay
    } = this.state;
    const { hasNext } = this.props;
    const title = isEdit ? "Edit ISCIs" : "Add ISCIs";
    const popoverIsciEditor = (
      <Popover id="popover-positioned-scrolling-top" title={title}>
        <FormGroup controlId="isciEditor" validationState={isValid}>
          <ControlLabel>
            House ISCI*,Client ISCI*, Day Include*, Brand{" "}
            <span
              style={{
                fontWeight: "normal",
                fontSize: "11px",
                color: "#999999"
              }}
            >
              <br />
              Use (m) to denote a married house ISCI. Use - between days in Day
              Include (M-T-W).
            </span>
          </ControlLabel>
          <FormControl
            componentClass="textarea"
            placeholder="Enter ISCIs"
            style={{ height: "100px" }}
            onChange={this.onChangeIscis}
            value={iscisValue}
          />
          {isValid != null && (
            <HelpBlock>
              <div className="text-danger" style={{ fontSize: 11 }}>
                {validationErrors}
              </div>
            </HelpBlock>
          )}
        </FormGroup>
        {hasNext && (
          <ButtonToolbar
            style={{
              marginLeft: "5px",
              marginBottom: "8px",
              float: "right",
              borderLeft: "1px solid #DDDDDD"
            }}
          >
            <Button
              bsStyle="success"
              bsSize="small"
              onClick={this.onSaveIscisNext}
            >
              Next
            </Button>
          </ButtonToolbar>
        )}
        <ButtonToolbar style={{ marginBottom: "8px", float: "right" }}>
          <Button bsStyle="default" bsSize="small" onClick={this.closePopover}>
            Cancel
          </Button>
          <Button bsStyle="success" bsSize="small" onClick={this.onSaveIscis}>
            Ok
          </Button>
        </ButtonToolbar>
      </Popover>
    );
    // revise to show house display if edit mode with tip
    const button = isEdit ? (
      <Button bsStyle="link" style={{ padding: "2px", fontSize: "11px" }}>
        <div className="truncate-iscis">{iscisDisplay.join(" | ")}</div>
      </Button>
    ) : (
      <Button bsStyle="link" style={{ padding: "2px", fontSize: "11px" }}>
        <Glyphicon style={{ marginRight: "6px" }} glyph="plus" />
        Add ISCIs
      </Button>
    );
    const tooltip = (
      <Tooltip id="Iscistooltip">
        <span style={{ fontSize: "9px" }}>
          ISCIs <br />
          {iscisValue}
        </span>
      </Tooltip>
    );
    // const touchedClass = (this.state.isChanged && isISCIEdited) ? 'editable-cell-changed' : '';
    const touchedClass = "";
    return (
      <div className={`${touchedClass} isci-cell`}>
        {isEdit && (
          <OverlayTrigger placement="top" overlay={tooltip}>
            <Button bsStyle="link" style={{ fontSize: "11px", padding: "2px" }}>
              <Glyphicon style={{ color: "#999" }} glyph="info-sign" />
            </Button>
          </OverlayTrigger>
        )}
        <OverlayTrigger
          trigger="click"
          placement="bottom"
          overlay={popoverIsciEditor}
          rootClose
          ref={ref => {
            this.popover = ref;
          }}
        >
          {button}
        </OverlayTrigger>
      </div>
    );
  }
}

GridIsciCell.defaultProps = {
  saveInputIscis: () => {}
};

GridIsciCell.propTypes = {
  Iscis: PropTypes.array.isRequired,
  saveInputIscis: PropTypes.func,
  hasNext: PropTypes.bool.isRequired,
  weekCnt: PropTypes.number.isRequired

  // isISCIEdited: PropTypes.bool.isRequired,
  // toggleEditIsciClass: PropTypes.func.isRequired,
};
