$(document).ready(function () {
    const cartIcon = $("#cartIcon");
    const cartItemsContainer = $("#cartItemsContainer");
    const cartItemCount = $("#cartItemCount");
    const userId = $("#userId").val();

    // Fetch the cart item count when the page loads
    fetchCartItemCount();

    // Event: Clicking the cart icon opens the modal and fetches cart items
    cartIcon.on("click", function () {
        fetchCartItems();
        $("#shoppingCartModal").modal("show");
    });

    // Fetch items for the cart
    function fetchCartItems() {
        if (!userId) {
            console.error("User ID is missing.");
            return;
        }

        $.ajax({
            url: `/ShoppingCart/GetCart?userId=${userId}`,
            method: "GET",
            success: function (response) {
                console.log("API Response:", response);
                if (response.items && response.items.length > 0) {
                    populateCart(response.items);
                } else {
                    handleEmptyCart();
                }
            },
            error: function (err) {
                console.error("Error fetching cart items", err);
                handleEmptyCart(); // Gracefully handle errors
            }
        });
    }

    // Populate cart items in the modal
    function populateCart(items) {
        let html = `<ul class="list-group">`;
        items.forEach(item => {
            html += `
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    Menu Item ID: ${item.menuItemId} (Quantity: ${item.quantity}) 
                    <span>$${item.price.toFixed(2)}</span>
                    <button class="btn btn-danger btn-sm remove-item" data-id="${item.menuItemId}">
                        <i class="bi bi-trash"></i>
                    </button>
                </li>`;
        });
        html += `</ul>`;
        cartItemsContainer.html(html);

        // Attach event handlers to remove buttons
        $(".remove-item").on("click", function () {
            const itemId = $(this).data("id");
            removeCartItem(itemId);
        });
    }

    // Handle removing an item from the cart
    function removeCartItem(itemId) {
        if (!userId) {
            console.error("User ID is missing.");
            return;
        }

        $.ajax({
            url: `/ShoppingCart/RemoveItemFromCart?userId=${userId}&menuItemId=${itemId}`,
            method: "DELETE",
            success: function () {
                fetchCartItems(); // Refresh the cart items
                fetchCartItemCount(); // Update badge count after removal
            },
            error: function (err) {
                console.error("Error removing cart item", err);
            }
        });
    }

    // Fetch the cart item count
    function fetchCartItemCount() {
        if (!userId) {
            console.error("User ID is missing.");
            cartItemCount.text(0); // Default to 0 if no user ID
            return;
        }

        $.ajax({
            url: `/ShoppingCart/GetCart?userId=${userId}`,
            method: "GET",
            success: function (response) {
                if (response.items && response.items.length > 0) {
                    cartItemCount.text(response.items.length); // Update the badge count
                } else {
                    cartItemCount.text(0); // Set to 0 if cart is empty
                }
            },
            error: function (err) {
                console.error("Error fetching cart item count", err);
                cartItemCount.text(0); // Default to 0 on error
            }
        });
    }

    // Handle when the cart is empty
    function handleEmptyCart() {
        cartItemsContainer.html('<p>Your cart is empty.</p>');
        cartItemCount.text(0); // Reset badge count
    }
});
