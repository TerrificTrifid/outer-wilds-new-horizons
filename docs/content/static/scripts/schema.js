function flashElement(t) {
    myElement = document.getElementById(t).parentNode, myElement.classList.add("jsfh-animated-property"), setTimeout(function () {
        myElement.classList.remove("jsfh-animated-property");
    }, 1e3);
}

function setAnchor(t) {
    history.pushState({}, "", t);
}

function anchorOnLoad() {
    let t = window.location.hash.split("?")[0].split("&")[0];
    "#" === t[0] && (t = t.substr(1)), t.length > 0 && anchorLink(t);
}

function anchorLink(t) {
    $("#" + t).parents().addBack().filter(".collapse:not(.show), .tab-pane, [role='tab']").each(function (t) {
        if ($(this).hasClass("collapse")) $(this).collapse("show"); else if ($(this).hasClass("tab-pane")) {
            const t = $("a[href='#" + $(this).attr("id") + "']");
            t && t.tab("show");
        } else "tab" === $(this).attr("role") && $(this).tab("show");
    }), setTimeout(function () {
        let e = document.getElementById(t);
        e && (e.scrollIntoView({block: "center", behavior: "smooth"}), setTimeout(function () {
            flashElement(t);
        }, 500));
    }, 1e3);
}

$(document).on("click", 'a[href^="#"]', function (t) {
    t.preventDefault(), history.pushState({}, "", this.href);
});