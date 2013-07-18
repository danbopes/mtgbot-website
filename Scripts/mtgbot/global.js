function pad(n, width, z) {
    z = z || '0';
    n = n + '';
    return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
}

function thumbLink(card) {
    return "http://mtgbot.tv/Images/cards/" + card.CardSet.GathererSet + "/" + pad(card.MagicCardsInfoId, 3) + ".thumb.jpg";
    //return "/Images/cards/" + card.CardSet.GathererSet + "/" + pad(card.MagicCardsInfoId, 3) + ".thumb.jpg";
}

function fullLink(card) {
    return "http://mtgbot.tv/Images/cards/" + card.CardSet.GathererSet + "/" + pad(card.MagicCardsInfoId, 3) + ".full.jpg";
    //return "/Images/cards/" + card.CardSet.GathererSet + "/" + pad(card.MagicCardsInfoId, 3) + ".full.jpg";
}

if (typeof window.console === "undefined") {
    window.console = {
        log: function () {
        }
    };
}