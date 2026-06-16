window.showLoader = function () {
    document
        .getElementById("global-loader")
        ?.classList.remove("d-none");
};

window.hideLoader = function () {
    document
        .getElementById("global-loader")
        ?.classList.add("d-none");
};