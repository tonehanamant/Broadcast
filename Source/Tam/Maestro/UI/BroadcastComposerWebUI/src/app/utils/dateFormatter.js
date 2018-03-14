export const getDateInFormat = (fullDate) => {
    const today = new Date(fullDate);

    let dd = today.getDate();
    let mm = today.getMonth() + 1; // January is 0!
    const yyyy = today.getFullYear();

    if (dd < 10) {
        dd = `0${dd}`;
    }

    if (mm < 10) {
        mm = `0${mm}`;
    }

    return `${mm}/${dd}/${yyyy}`;
};

export const getDateForDisplay = (list) => {
    const newList = list.map((item) => {
        const displayItem = {};
        displayItem.Display = `${getDateInFormat(item.FlightStartDate)} - ${getDateInFormat(item.FlightStartDate)} | ${item.DayPart} | ${item.SpotLength}`;
        displayItem.Id = item.Id;
        return displayItem;
    });
    return newList;
};
