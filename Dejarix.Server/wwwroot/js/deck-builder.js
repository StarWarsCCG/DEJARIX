const cardSearchTitle = $("#card-search-title");
const cardSearchTableBody = $("#card-search-table-body");
const cardSearchMessage = $("#card-search-message");
const cardSearchButton = $("#card-search-button");

function searchNormalized(text) {
    return String(text).toLowerCase().replace(/[^a-z0-9]/gi, '');
}

function doQuery() {
    const searchString = searchNormalized(cardSearchTitle.val());
    cardSearchMessage.empty();
    cardSearchMessage.removeClass();
    
    if (searchString && searchString.length > 2) {
        cardSearchTableBody.empty();

        starWarsCards.forEach(function(item, index) {
            if (item.CardNameNormalized.includes(searchString)) {
                cardSearchTableBody.append(
                    '<tr><td><img src="/images/expansions/' + item.Expansion + '.png?v=2" title="' + item.Expansion + '" alt="" /></td><td>' + item.CardName + '</td><td class="text-sm-right">5</td></tr>');
            }
        });
        // $.ajax({
        //     url: "/ScompLink/Cards?title=" + encodeURI(searchString),
        //     error: function(xhr, status, thrown) {
        //         cardSearchMessage.addClass("alert alert-danger");
        //         cardSearchMessage.text("Error! Unable to complete request.");
        //         console.error(thrown);
        //     },
        //     success: function(result) {
        //         console.log(result);
        //         cardSearchTableBody.empty();
        //         result.forEach(function(item, index) {
        //             cardSearchTableBody.append(
        //                 '<tr><td><img src="/images/expansions/' + item.expansion + '.png?v=2" title="' + item.expansion + '" alt="" /></td><td>' + item.title + '</td><td class="text-sm-right">5</td></tr>');
        //         });
        //     }
        // });
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
