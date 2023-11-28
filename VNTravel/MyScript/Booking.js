let hotelContent = document.querySelector(".hotel-content");
let openhotel = document.querySelector(".open-hotel");
let closehotel = document.querySelector(".close-hotel");

let flightContent = document.querySelector(".flight-content");
let openflight = document.querySelector(".open-flight");
let closeflight = document.querySelector(".close-flight");

openhotel.addEventListener("click", function () {
    hotelContent.classList.remove("hidden-hotel");
    flightContent.classList.add("hidden-flight");
});

closehotel.addEventListener("click", function () {
    hotelContent.classList.add("hidden-hotel");
});

openflight.addEventListener("click", function () {
    flightContent.classList.remove("hidden-flight");
    hotelContent.classList.add("hidden-hotel");
});

closeflight.addEventListener("click", function () {
    flightContent.classList.add("hidden-flight");
});

document.addEventListener("keydown", function (event) {
    if (event.key === "Escape"
        && !hotelContent.classList.contains("hidden-hotel")
    ) {
        hotelContent.classList.add("hidden-hotel");
    } else if (event.key === "Escape"
        && !flightContent.classList.contains("hidden-flight")
    ) {
        flightContent.classList.add("hidden-flight");
    }
});

openhotel.addEventListener("click", function () {
    openhotel.style.backgroundColor = "royalblue";
    closehotel.style.backgroundColor = "white";
    openhotel.style.color = "white";
    openflight.style.backgroundColor = "white";
    openflight.style.color = "black";
});

openflight.addEventListener("click", function () {
    openflight.style.backgroundColor = "royalblue";
    closeflight.style.backgroundColor = "white";
    openflight.style.color = "white";
    openhotel.style.backgroundColor = "white";
    openhotel.style.color = "black";
});