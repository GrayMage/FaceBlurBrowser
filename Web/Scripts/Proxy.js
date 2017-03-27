function getAbsoluteUrl(url) {
    if (new RegExp("^https?:\/\/").exec(url)) {
        return url;
    }

    if (new RegExp("^\/\/").exec(url)) {
        return "http:" + url;
    }

    var targetUrl = decodeURIComponent((new RegExp(".*?q=([^&]*)")).exec(location.href)[1]);
    return targetUrl + url;
}

function isEmbeddedImage(url) {
    return url.substring(0, 10) === "data:image";
}

function isProxified(url) {
    var origin = window.location.origin;
    return url.substring(0, origin.length) === origin;
}

function getProxifiedLinkUrl(url) {
    if (isProxified(url)) {
        return url;
    }

    return window.location.origin + "/GetPage?q=" + encodeURIComponent(getAbsoluteUrl(url));
}

function getProxifiedImageUrl(url) {
    if (isProxified(url)) {
        return url;
    }

    return window.location.origin + "/GetImage?type=" +
    (isEmbeddedImage(url)
        ? "embedded&q=" + encodeURIComponent(url)
        : "url&q=" + encodeURIComponent(getAbsoluteUrl(url)));
}

function proxify(element) {
    if (element.hasAttribute && element.hasAttribute("style")) {
        var style = element.getAttribute("style");
        var match = new RegExp("background-image:\\s*url\\(\\\"(.*?)\\\"\\)").exec(style);
        if (match) {
            var url = match[1];
            var newStyle = style.replace(url, getProxifiedImageUrl(url));
            if (newStyle !== style) {
                element.setAttribute("style", newStyle);
            }
        }
    }
    switch (element.nodeName.toLowerCase()) {
    case "img":
        if (element.src) {
            var newSrc = getProxifiedImageUrl(element.src);
            if (newSrc !== element.src) {
                element.src = newSrc;
            }
        }
        break;
    case "a":
        if (element.href) {
            var newHref = getProxifiedLinkUrl(element.href);
            if (newHref !== element.href) {
                element.href = newHref;
            }
        }
        break;
    }
}

function processMutations(summaries) {
    summaries.forEach(function(summary) {
        summary.added.forEach(proxify);
        var changedAttributes = summary.attributeChanged;
        for (var obj in changedAttributes) {
            if (changedAttributes.hasOwnProperty(obj)) {
                changedAttributes[obj].forEach(proxify);
            }
        }
    });
};

var MutationObserver = window.MutationObserver || window.WebKitMutationObserver || window.MozMutationObserver;

var observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        var addedNodes = mutation.addedNodes;
        for (var obj in addedNodes) {
            if (addedNodes.hasOwnProperty(obj)) {
                proxify(addedNodes[obj]);
            }
        }
        proxify(mutation.target);
    });
});

observer.observe(document.querySelector("html"),
{
    attributes: true,
    childList: true,
    subtree: true
});