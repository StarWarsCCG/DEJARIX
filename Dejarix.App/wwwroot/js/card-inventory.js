const cardInventoryBody = $('#card-inventory-body');
var currentInventory = null;

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
    } else if (results.length == 0) {
        cardSearchMessage.addClass("alert alert-warning");
        cardSearchMessage.text("No results.");
    } else {
        cardSearchMessage.addClass("alert alert-success");
        cardSearchMessage.text(results.length + " results.");
    }

    results.sort(compareByName);

    const htmlResults = [];

    results.forEach(function(item) {
        const uniqueness = item.hasOwnProperty("Uniqueness") ? item.Uniqueness : '';
        const fullTitle = escapeHtml(uniqueness + item.CardName);
        htmlResults.push(
            '<tr id="card-' +
            item.ImageId +
            '" data-front-id="' +
            item.ImageId +
            '" data-back-id="' +
            item.OtherImageId +
            '"><td class="card-preview" data-front-id="' +
            item.ImageId +
            '" data-back-id="' +
            item.OtherImageId +
            '">' +
            getExpansionImage(item) +
            ' ' +
            fullTitle +
            '</td><td>private</td><td>public</td></tr>');
    });

    cardSearchTableBody.append(htmlResults.join(''));
    $("#card-search-table-body .card-preview").click(function(e) {
        const data = e.currentTarget.dataset;
        activeCardFront = data.frontId;
        activeCardBack = data.backId;
        cardSearchImage.empty();
        cardSearchImage.append(
            '<img id="card-search-preview" src="/images/cards/png-370x512/' +
            activeCardFront +
            '.png" alt="" />');
        cardSearchModal.modal({keyboard:true,focus:true,show:true});

        $('#card-search-preview').click(function(ee) {
            const swapValue = activeCardFront;
            activeCardFront = activeCardBack;
            activeCardBack = swapValue;

            ee.currentTarget.src = '/images/cards/png-370x512/' + activeCardFront + '.png';
        });
    });

    // $("#card-search-table-body .card-add").click(function (e) {
    //     let id = e.currentTarget.dataset.frontId;
    //     let card = cardsById[id];
        
    //     if (!card.IsFront) {
    //         id = card.OtherImageId;
    //         card = cardsById[id];
    //     }

    //     const uniqueness = card.hasOwnProperty("Uniqueness") ? card.Uniqueness : '';
    //     const fullTitle = escapeHtml(uniqueness + card.CardName);

    //     cardDeckTableBody.append(
    //         '<tr><td>' +
    //         getExpansionImage(card) +
    //         '</td><td>' +
    //         fullTitle +
    //         '</td></tr>');
    // });
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
    url: "/scomp-link/card-inventory",
    success: function(result) {
        currentInventory = result;
        console.debug('Inventory retrieved!');
    },
    error: function(jqXHR, textStatus, errorThrown) {
        console.error(textStatus);
    }
});
