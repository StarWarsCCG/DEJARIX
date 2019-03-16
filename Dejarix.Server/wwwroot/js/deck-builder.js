const cardSearchTitle = $("#card-search-title");
const cardSearchTypeFilter = $("#card-search-type-filter");
const cardSearchExpansionFilter = $("#card-search-expansion-filter");
const cardSearchTableBody = $("#card-search-table-body");
const cardSearchMessage = $("#card-search-message");
const cardSearchButton = $("#card-search-button");
const cardSearchImage = $("#card-search-image");
var starWarsCards = null;

function areEqual(str1, str2) {
    return String(str1).localeCompare(str2) == 0;
}

function isCardType(card, primaryType, secondaryType) {
    return areEqual(card.PrimaryType, primaryType) && card.SecondaryTypes.includes(secondaryType);
}

function compareByName(card1, card2) {
    return card1.CardName.localeCompare(card2.CardName);
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

function searchNormalized(text) {
    return String(text).toLowerCase().replace(/[^a-z0-9]/gi, '');
}

function doQuery() {
    if (areEqual(cardSearchButton.attr("disabled"), "disabled")) {
        return;
    }

    const titleSearch = searchNormalized(cardSearchTitle.val());
    cardSearchMessage.empty();
    cardSearchMessage.removeClass();
    
    cardSearchTableBody.empty();

    const results = [];
    const filters = [c => results.length <= 100];
    const typeFilter = cardSearchTypeFilter.val();

    if (titleSearch) {
        filters.push(c => c.CardNameNormalized.includes(titleSearch));
    }

    if (typeFilterMap.hasOwnProperty(typeFilter)) {
        filters.push(typeFilterMap[typeFilter]);
    }

    const expansionFilter = cardSearchExpansionFilter.val();

    if (expansionFilter && !expansionFilter.startsWith('[')) {
        filters.push(c => areEqual(c.Expansion, expansionFilter));
    }

    starWarsCards.forEach(function(item) {
        for (let filter of filters) {
            if (!filter(item)) {
                return;
            }
        }

        results.push(item);
    });

    if (results.length > 100) {
        results.pop();
        cardSearchMessage.addClass("alert alert-warning");
        cardSearchMessage.text("Results limited to 100.");
    }

    results.sort(compareByName);

    const htmlResults = [];

    results.forEach(function(item) {
        const uniqueness = item.hasOwnProperty("Uniqueness") ? item.Uniqueness : '';
        htmlResults.push(
            '<tr id="' +
            item.ImageId +
            '"><td><img src="/images/expansions/' +
            item.Expansion.replace('#', '') +
            '.png?v=2" title="' +
            item.Expansion +
            '" alt="" /></td><td>' +
            uniqueness +
            item.CardName +
            '</td><td class="text-sm-right">' +
            item.Destiny +
            '</td></tr>');
    });

    cardSearchTableBody.append(htmlResults.join(''));
    $("#card-search-table-body tr").click(function(e) {
        cardSearchImage.empty();
        cardSearchImage.append(
            '<img src="/images/cards/' +
            e.currentTarget.id +
            '.jpg" alt="" />');
        // console.log(e);
    });
    if (titleSearch) {
    } else {
        // cardSearchMessage.addClass("alert alert-warning");
        // cardSearchMessage.text("Requires 3 characters to search.");
    }
}

cardSearchTitle.keypress(function(e) {
    if (e.which == 13) {
        e.preventDefault();
        // alert(cardSearchTitle.val());
        doQuery();
    }
});

cardSearchButton.click(doQuery);
cardSearchTypeFilter.change(doQuery);
cardSearchExpansionFilter.change(doQuery);
cardSearchTitle.focus();

$.ajax({
    url: "/ScompLink/AllCards",
    success: function(result) {
        starWarsCards = result;
        cardSearchButton.removeAttr("disabled");

        cardSearchMessage.addClass("alert alert-success");
        cardSearchMessage.text("Database loaded.");
    },
    error: function() {
        cardSearchMessage.addClass("alert alert-danger");
        cardSearchMessage.text("Error retrieving card database.");
    }
});

