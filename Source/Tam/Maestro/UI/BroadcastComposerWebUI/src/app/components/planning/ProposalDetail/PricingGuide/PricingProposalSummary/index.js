import React, { Component } from "react";
import PropTypes from "prop-types";

import { Table, OverlayTrigger, Tooltip, Glyphicon } from "react-bootstrap";
import numeral from "numeral";
import DateMDYYYY from "Patterns/TextFormatters/DateMDYYYY";

const findValue = (options, id) => options.find(option => option.Id === id);

export default class PricingProposalSummary extends Component {
  constructor(props) {
    super(props);
    this.getProposalSummaryData = this.getProposalSummaryData.bind(this);
  }

  getProposalSummaryData() {
    const { initialdata, detail, proposalEditForm } = this.props;
    // if (!detail) return {};
    const advertiser = findValue(
      initialdata.Advertisers,
      proposalEditForm.AdvertiserId
    );
    const postType = findValue(
      initialdata.SchedulePostTypes,
      proposalEditForm.PostType
    );
    const guaranteed = findValue(
      initialdata.Audiences,
      proposalEditForm.GuaranteedDemoId
    );
    const spotLength = findValue(initialdata.SpotLengths, detail.SpotLengthId);
    const secDemos = [];
    proposalEditForm.SecondaryDemos.forEach(id => {
      const demo = findValue(initialdata.Audiences, id);
      secDemos.push(demo.Display);
    });
    // secondary with tips if more than 2 - see render
    // const secondary = secDemos.length ? secDemos.join(", ") : "--";
    // display with no more than 2 values; if additional use tip
    /*     const secondaryDisplay = secDemos.length
      ? secDemos.slice(0, 2).join(", ")
      : "--"; */
    const hasSecondaryTips = secDemos.length > 2;
    const months = initialdata.ForecastDefaults.CrunchedMonths;
    const projectionShare = findValue(months, detail.ShareProjectionBookId);
    const projectionHut = findValue(months, detail.HutProjectionBookId);
    const ret = {
      name: proposalEditForm.ProposalName,
      id: proposalEditForm.Id,
      advertiser: advertiser.Display,
      coverage: proposalEditForm.MarketCoverage,
      postType: postType.Display,
      equivalized: proposalEditForm.Equivalized,
      guaranteed: guaranteed.Display,
      // secondary: secondaryDisplay,
      // secondaryTipItems: secDemos,
      secondaryItems: secDemos,
      hasSecondaryTips,
      // from detail
      flightStart: detail.FlightStartDate, // use DateMDYYYY in render
      flightEnd: detail.FlightEndDate,
      daypart: detail.Daypart.Text,
      spotLength: spotLength.Display,
      projectionShare, // if defined use Display
      projectionHut // if defined use Display
    };
    return ret;
  }

  render() {
    const proposalSummary = this.getProposalSummaryData();
    const hasTips = proposalSummary.hasSecondaryTips;
    const secondaryItems = proposalSummary.secondaryItems;
    const secondaryDisplayItems = secondaryItems.length
      ? secondaryItems.slice(0, 2)
      : [];
    if (hasTips) secondaryDisplayItems.push("...");
    const secondaryDisplay = secondaryDisplayItems.length
      ? secondaryDisplayItems.join(", ")
      : "--";
    const tips = hasTips ? secondaryItems.join(", ") : "";
    const secondaryTip = <Tooltip id="secondarytooltip">{tips}</Tooltip>;

    // console.log("get proposal summary >>>>>>", proposalSummary);
    return (
      <div style={{ marginTop: "8px" }}>
        <Table condensed style={{ marginBottom: "0px" }}>
          <thead>
            <tr>
              <th>PROPOSAL</th>
              <th className="cardLabel">Advertiser</th>
              <th className="cardLabel">Coverage</th>
              <th className="cardLabel">Post Type</th>
              <th className="cardLabel">Equivalized</th>
              <th className="cardLabel">Guaranteed</th>
              <th className="cardLabel">Secondary</th>
              <th className="cardLabel">Flight</th>
              <th className="cardLabel">Daypart</th>
              <th className="cardLabel">Spot Length</th>
              <th className="cardLabel">Share</th>
              <th className="cardLabel">Hut</th>
            </tr>
          </thead>
          <tbody style={{ fontSize: "11px" }}>
            <tr>
              <td>
                <strong>
                  {proposalSummary.name} ({proposalSummary.id})
                </strong>
              </td>
              <td>
                <strong>{proposalSummary.advertiser}</strong>
              </td>
              <td>
                <strong>
                  {proposalSummary.coverage
                    ? numeral(proposalSummary.coverage * 100).format("0,0.[00]")
                    : "--"}%
                </strong>
              </td>
              <td>
                <strong>{proposalSummary.postType}</strong>
              </td>
              <td>
                <strong>{proposalSummary.equivalized ? "Yes" : "No"}</strong>
              </td>
              <td>
                <strong>{proposalSummary.guaranteed}</strong>
              </td>
              <td>
                <strong>{secondaryDisplay}</strong>
                {proposalSummary.hasSecondaryTips && (
                  <OverlayTrigger placement="top" overlay={secondaryTip}>
                    <Glyphicon
                      style={{
                        marginLeft: "4px",
                        fontWeight: "bold",
                        color: "#999"
                      }}
                      // glyph="option-horizontal"
                      glyph="info-sign"
                    />
                  </OverlayTrigger>
                )}
              </td>
              <td>
                <strong>
                  <span>
                    <DateMDYYYY date={proposalSummary.flightStart} />
                    <span> - </span>
                    <DateMDYYYY date={proposalSummary.flightEnd} />
                  </span>
                </strong>
              </td>
              <td>
                <strong>{proposalSummary.daypart}</strong>
              </td>
              <td>
                <strong>{proposalSummary.spotLength}</strong>
              </td>
              <td>
                <strong>
                  {proposalSummary.projectionShare
                    ? proposalSummary.projectionShare.Display
                    : "--"}
                </strong>
              </td>
              <td>
                <strong>
                  {proposalSummary.projectionHut
                    ? proposalSummary.projectionHut.Display
                    : "--"}
                </strong>
              </td>
            </tr>
          </tbody>
        </Table>
      </div>
    );
  }
}

PricingProposalSummary.defaultProps = { detail: {} };

PricingProposalSummary.propTypes = {
  // detail: PropTypes.PropTypes.object.isRequired,
  detail: PropTypes.object,
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired
};
