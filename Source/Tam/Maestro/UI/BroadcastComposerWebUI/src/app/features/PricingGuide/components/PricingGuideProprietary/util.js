import React from "react";
import { FormControl, Label } from "react-bootstrap";
import { InputNumber } from "antd";
import numeral from "numeral";

export const transformInvetorySrc = (
  inventorySrc,
  props,
  values,
  onChange,
  isEditing
) =>
  inventorySrc.map(i => {
    const cpm = props[`propCpm${i.Display}`];
    const localCpm = values[`editingPropCpm${i.Id}`];
    const impression = props[`propImpressions${i.Display}`];
    const localImpression = values[`editingPropImpressions${i.Id}`];
    return (
      <tr key={`invetory-row#${i.Id}`}>
        <td>{i.Display}</td>
        <td>
          {!isEditing && (
            <FormControl.Static>
              {impression ? numeral(impression * 100).format("0,0.[00]") : "--"}
              %
            </FormControl.Static>
          )}
          {isEditing && (
            <InputNumber
              defaultValue={localImpression * 100}
              disabled={props.isReadOnly}
              min={0}
              max={100}
              precision={2}
              // style={{ width: '100px' }}
              formatter={value =>
                `% ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
              }
              parser={value => value.replace(/%\s?|(,*)/g, "")}
              onChange={value => {
                onChange(`editingPropImpressions${i.Id}`, value / 100);
              }}
            />
          )}
        </td>
        <td>
          {isEditing && (
            <InputNumber
              defaultValue={localCpm || null}
              disabled={props.isReadOnly}
              min={0}
              precision={2}
              style={{ width: "100%" }}
              formatter={value =>
                `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ",")
              }
              parser={value => value.replace(/\$\s?|(,*)/g, "")}
              onChange={value => {
                onChange(`editingPropCpm${i.Id}`, value);
              }}
            />
          )}
          {!isEditing && (
            <FormControl.Static>
              ${cpm ? numeral(cpm).format("0,0.[00]") : "--"}
            </FormControl.Static>
          )}
        </td>
      </tr>
    );
  });

export const transformInvetoryLabel = (inventorySrc, props) =>
  inventorySrc.map(i => (
    <Label
      key={`invetory-label#${i.Id}`}
      className={
        props[`propImpressions${i.Display}`] > 0
          ? "tag-label active"
          : "tag-label inactive"
      }
    >
      {i.Display}
    </Label>
  ));
