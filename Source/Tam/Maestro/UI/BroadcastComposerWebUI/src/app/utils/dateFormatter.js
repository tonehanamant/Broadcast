import moment from "moment";

export const getDateInFormat = (fullDate, withTime, timeOnly) => {
  const today = new Date(fullDate);

  let dd = today.getDate();
  let mm = today.getMonth() + 1; // January is 0!
  const yyyy = today.getFullYear();

  let hours = today.getHours();
  let minutes = today.getMinutes();
  let seconds = today.getSeconds();
  let day = "AM";

  let date;

  if (dd < 10) {
    dd = `0${dd}`;
  }

  if (mm < 10) {
    mm = `0${mm}`;
  }

  date = `${mm}/${dd}/${yyyy}`;

  if (hours > 11) {
    day = "PM";
    hours -= 12;
  }

  if (hours < 10) {
    hours = `0${hours}`;
  }

  if (minutes < 10) {
    minutes = `0${minutes}`;
  }
  if (seconds < 10) {
    seconds = `0${seconds}`;
  }

  if (withTime) {
    date = `${mm}/${dd}/${yyyy} ${hours}:${minutes}:${seconds} ${day}`;
  }

  const result = timeOnly ? `${hours}:${minutes}:${seconds} ${day}` : date;

  return result;
};

export const getDateForDisplay = list => {
  const newList = list.map(item => {
    const displayItem = {};
    displayItem.Display = `${getDateInFormat(
      item.FlightStartDate
    )} - ${getDateInFormat(item.FlightEndDate)} | ${item.DayPart} | ${
      item.SpotLength
    }`;
    displayItem.Id = item.Id;
    return displayItem;
  });
  return newList;
};

export const getDay = date => {
  const days = [
    "Sunday",
    "Monday",
    "Tuesday",
    "Wednesday",
    "Thursday",
    "Friday",
    "Saturday"
  ];

  return days[date];
};

export const getSecondsToTimeString = time => {
  let seconds = time % 60;
  seconds = parseInt(seconds, 10); // radix 10
  let minutes = (time / 60) % 60;
  minutes = parseInt(minutes, 10);
  let hours = (time / (60 * 60)) % 24;
  hours = parseInt(hours, 10);

  const t = new Date(1970, 0, 2, hours, minutes, seconds, 0);

  // return moment(t).format('h:mmA').replace(':00', '');
  return moment(t).format("hh:mm:ss A");
};
