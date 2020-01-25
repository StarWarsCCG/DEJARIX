const cardInventoryBody = $('#card-inventory-body');
var currentInventory = null;
var starWarsCards = null;
var cardsById = {};

$.ajax({
    url: "/scomp-link/all-cards",
    success: function(result) {
        starWarsCards = result;
        starWarsCards.forEach(item => cardsById[item.ImageId] = item);
        console.debug('All cards retrieved!');
    },
    error: function(jqXHR, textStatus, errorThrown) {
        console.error(textStatus);
    }
});

$.ajax({
    url: "/scomp-link/card-inventory",
    success: function(result) {
        currentInventory = result;
        console.debug('Inventory retrieved!');
    },
    error: function(jqXHR, textStatus, errorThrown) {
        console.error(textStatus);
    }
});
