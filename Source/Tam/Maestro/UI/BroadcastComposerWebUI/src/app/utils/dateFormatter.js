export const getDateInFormat = (fullDate, withTime) => {
    const today = new Date(fullDate);

    let dd = today.getDate();
    let mm = today.getMonth() + 1; // January is 0!
    const yyyy = today.getFullYear();

    let hours = today.getHours();
    let minutes = today.getMinutes();
    let seconds = today.getSeconds();
    let day = 'AM';

    let date;

    if (dd < 10) {
        dd = `0${dd}`;
    }

    if (mm < 10) {
        mm = `0${mm}`;
    }

    date = `${mm}/${dd}/${yyyy}`;

    if (hours > 11) {
        day = 'PM';
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

    return date;
};

export const getDateForDisplay = (list) => {
    const newList = list.map((item) => {
        const displayItem = {};
        displayItem.Display = `${getDateInFormat(item.FlightStartDate)} - ${getDateInFormat(item.FlightEndDate)} | ${item.DayPart} | ${item.SpotLength}`;
        displayItem.Id = item.Id;
        return displayItem;
    });
    return newList;
};
