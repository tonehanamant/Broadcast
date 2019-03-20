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
      secondaryItems: secDemos,
      hasSecondaryTips,
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
    const { hasSecondaryTips: hasTips, secondaryItems } = proposalSummary;
    const secondaryDisplayItems = secondaryItems.length
      ? secondaryItems.slice(0, 2)
      : [];
    if (hasTips) secondaryDisplayItems.push("...");
    const secondaryDisplay = secondaryDisplayItems.length
      ? secondaryDisplayItems.join(", ")
      : "--";
    const tips = hasTips ? secondaryItems.join(", ") : "";
    const secondaryTip = <Tooltip id="secondarytooltip">{tips}</Tooltip>;

    return (
      <div style={{ marginTop: "8px" }}>
        <Table condensed style={{ marginBottom: "0px" }}>
          <thead>
            <tr>
              <th>PROPOSAL</th>
              <th styleName="cardLabel">Advertiser</th>
              <th styleName="cardLabel">Coverage</th>
              <th styleName="cardLabel">Post Type</th>
              <th styleName="cardLabel">Equivalized</th>
              <th styleName="cardLabel">Guaranteed</th>
              <th styleName="cardLabel">Secondary</th>
              <th styleName="cardLabel">Flight</th>
              <th styleName="cardLabel">Daypart</th>
              <th styleName="cardLabel">Spot Length</th>
              <th styleName="cardLabel">Share</th>
              <th styleName="cardLabel">Hut</th>
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
                    : "--"}
                  %
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
  detail: PropTypes.object,
  proposalEditForm: PropTypes.object.isRequired,
  initialdata: PropTypes.object.isRequired
};
