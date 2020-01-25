const cardSearchImage = $("#card-search-image");
const cardSearchModal = $('#card-search-modal');
const cardSearchTitle = $("#card-search-title");
const cardSearchTypeFilter = $("#card-search-type-filter");
const cardSearchExpansionFilter = $("#card-search-expansion-filter");
const cardSearchButton = $("#card-search-button");
const cardSearchMessage = $("#card-search-message");
const cardSearchTableBody = $("#card-search-table-body");
const cardsById = {};
var starWarsCards = null;

function getExpansionImage(card) {
    return '<img src="/images/expansions/' +
        card.Expansion.replace('#', '') +
        '.png?v=2" title="' +
        card.Expansion +
        '" alt="" />';
}

// https://stackoverflow.com/a/12034334
var entityMap = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;',
    '/': '&#x2F;',
    '`': '&#x60;',
    '=': '&#x3D;'
};

function escapeHtml (string) {
    return String(string).replace(/[&<>"'`=\/]/g, function (s) {
        return entityMap[s];
    });
}

function compareByName(card1, card2) {
    return card1.CardName.localeCompare(card2.CardName);
}

function getFrontId(card) {
    return card.IsFront ? card.ImageId : card.OtherImageId;
}

function searchNormalized(text) {
    return String(text).toLowerCase().replace(/[^a-z0-9]/gi, '');
}

function areEqual(str1, str2) {
    return String(str1).localeCompare(String(str2)) == 0;
}

function isCardType(card, primaryType, secondaryType) {
    return areEqual(card.PrimaryType, primaryType) && card.SecondaryTypes.includes(secondaryType);
}

const typeFilterMap = {
    "admirals-order": c => areEqual(c.PrimaryType, "Admiral's Order"),
    "character": c => areEqual(c.PrimaryType, "Character"),
    "character-alien": c => isCardType(c, "Character", "Alien"),
    "character-dark-jedi-master": c => isCardType(c, "Character", "Dark Jedi Master"),
    "character-droid": c => isCardType(c, "Character", "Droid"),
    "character-first-order": c => isCardType(c, "Character", "First Order"),
    "character-imperial": c => isCardType(c, "Character", "Imperial"),
    "character-jedi-master": c => isCardType(c, "Character", "Jedi Master"),
    "character-rebel": c => isCardType(c, "Character", "Rebel"),
    "character-republic": c => isCardType(c, "Character", "Republic"),
    "character-resistance": c => isCardType(c, "Character", "Resistance"),
    "character-sith": c => isCardType(c, "Character", "Sith"),
    "creature": c => areEqual(c.PrimaryType, "Creature"),
    "defensive-shield": c => areEqual(c.PrimaryType, "Defensive Shield"),
    "device": c => areEqual(c.PrimaryType, "Device"),
    "effect": c => areEqual(c.PrimaryType, "Effect"),
    "effect-immediate": c => isCardType(c, "Effect", "Immediate"),
    "effect-mobile": c => isCardType(c, "Effect", "Mobile"),
    "effect-political": c => isCardType(c, "Effect", "Political"),
    "effect-starting": c => isCardType(c, "Effect", "Starting"),
    "effect-utinni": c => isCardType(c, "Effect", "Utinni"),
    "epic-event": c => areEqual(c.PrimaryType, "Epic Event"),
    "interrupt": c => areEqual(c.PrimaryType, "Interrupt"),
    "interrupt-lost": c => isCardType(c, "Interrupt", "Lost"),
    "interrupt-starting": c => isCardType(c, "Interrupt", "Starting"),
    "interrupt-used": c => isCardType(c, "Interrupt", "Used"),
    "jedi-test": c => areEqual(c.PrimaryType, "Jedi Test"),
    "location": c => areEqual(c.PrimaryType, "Location"),
    "location-sector": c => isCardType(c, "Location", "Sector"),
    "location-site": c => isCardType(c, "Location", "Site"),
    "location-system": c => isCardType(c, "Location", "System"),
    "objective": c => areEqual(c.PrimaryType, "Objective"),
    "objective-front": c => areEqual(c.PrimaryType, "Objective") && c.IsFront,
    "objective-back": c => areEqual(c.PrimaryType, "Objective") && !c.IsFront,
    "podracer": c => areEqual(c.PrimaryType, "Podracer"),
    "starship": c => areEqual(c.PrimaryType, "Starship"),
    "starship-capital": c => isCardType(c, "Starship", "Capital"),
    "starship-squadron": c => isCardType(c, "Starship", "Squadron"),
    "starship-starfighter": c => isCardType(c, "Starship", "Starfighter"),
    "vehicle": c => areEqual(c.PrimaryType, "Vehicle"),
    "vehicle-combat": c => isCardType(c, "Vehicle", "Combat"),
    "vehicle-creature": c => isCardType(c, "Vehicle", "Creature"),
    "vehicle-shuttle": c => isCardType(c, "Vehicle", "Shuttle"),
    "vehicle-transport": c => isCardType(c, "Vehicle", "Transport"),
    "weapon": c => areEqual(c.PrimaryType, "Weapon"),
    "weapon-artillery": c => isCardType(c, "Weapon", "Artillery"),
    "weapon-automated": c => isCardType(c, "Weapon", "Automated"),
    "weapon-character": c => isCardType(c, "Weapon", "Character"),
    "weapon-death-star": c => isCardType(c, "Weapon", "Death Star"),
    "weapon-death-star-ii": c => isCardType(c, "Weapon", "Death Star II"),
    "weapon-starship": c => isCardType(c, "Weapon", "Starship"),
    "weapon-vehicle": c => isCardType(c, "Weapon", "Vehicle")
};

$.ajax({
    url: "/scomp-link/all-cards",
    success: function(result) {
        starWarsCards = result;
        starWarsCards.forEach(item => cardsById[item.ImageId] = item);
        cardSearchButton.removeAttr("disabled");

        cardSearchMessage.removeClass();
        cardSearchMessage.addClass("alert alert-success");
        cardSearchMessage.text("Database loaded.");
        console.debug('All cards retrieved!');
    },
    error: function(jqXHR, textStatus, errorThrown) {
        cardSearchMessage.removeClass();
        cardSearchMessage.addClass("alert alert-danger");
        cardSearchMessage.text("Error retrieving card database.");
        console.error(textStatus);
    }
});
