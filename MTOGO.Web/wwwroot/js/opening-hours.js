document.addEventListener("DOMContentLoaded", function () {
    if (typeof openingHours === "undefined") {
        console.error("Operating hours data is not available.");
        return;
    }

    const openStatusIndicator = document.getElementById("openStatusIndicator");

    const determineOpenStatus = () => {
        const now = new Date();
        const currentDay = now.getDay(); 
        const currentTimeInMinutes = now.getHours() * 60 + now.getMinutes(); 

        const todayHours = openingHours.find(h => h.Day === currentDay);

        if (!todayHours) {
            return false; 
        }

        const parseTimeSpanToMinutes = (timeSpan) => {
            const [hours, minutes] = timeSpan.split(":").map(Number);
            return hours * 60 + minutes; 
        };

        const openingMinutes = parseTimeSpanToMinutes(todayHours.OpeningHours);
        const closingMinutes = parseTimeSpanToMinutes(todayHours.ClosingHours);

        return currentTimeInMinutes >= openingMinutes && currentTimeInMinutes < closingMinutes;
    };

    const isOpen = determineOpenStatus();

    openStatusIndicator.innerHTML = isOpen
        ? '<span class="badge bg-success">Open</span>'
        : '<span class="badge bg-danger">Closed</span>';
});
