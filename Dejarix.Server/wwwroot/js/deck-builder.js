const cardSearchTitle = $("#card-search-title");
const cardSearchTypeFilter = $("#card-search-type-filter");
const cardSearchTableBody = $("#card-search-table-body");
const cardSearchMessage = $("#card-search-message");
const cardSearchButton = $("#card-search-button");

function areEqual(str1, str2) {
    return str1.localeCompare(str2) == 0;
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
    const searchString = searchNormalized(cardSearchTitle.val());
    cardSearchMessage.empty();
    cardSearchMessage.removeClass();
    
    if (searchString && searchString.length > 2) {
        cardSearchTableBody.empty();

        const filters = [function(c) { return c.CardNameNormalized.includes(searchString); }];
        const typeFilter = cardSearchTypeFilter.val();

        if (typeFilterMap.hasOwnProperty(typeFilter)) {
            filters.push(typeFilterMap[typeFilter]);
        }

        const results = [];

        starWarsCards.forEach(function(item) {
            let success = true;

            for (let filter of filters) {
                if (!filter(item)) {
                    success = false;
                    break;
                }
            }

            if (success) {
                results.push(item);
            }
        });

        const htmlResults = [];

        results.forEach(function(item) {
            const uniqueness = item.hasOwnProperty("Uniqueness") ? item.Uniqueness : '';
            htmlResults.push(
                '<tr><td><img src="/images/expansions/' + item.Expansion + '.png?v=2" title="' + item.Expansion + '" alt="" /></td><td>' + uniqueness + item.CardName + '</td><td class="text-sm-right">' + item.Destiny + '</td></tr>');
        });

        cardSearchTableBody.append(htmlResults.join(''));
    } else {
        cardSearchMessage.addClass("alert alert-warning");
        cardSearchMessage.text("Requires 3 characters to search.");
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
cardSearchTitle.focus();
