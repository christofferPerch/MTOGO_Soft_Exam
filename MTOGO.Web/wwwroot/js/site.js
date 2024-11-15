$(document).ready(function () {
    const cartIcon = $("#cartIcon");
    const cartItemsContainer = $("#cartItemsContainer");
    const cartItemCount = $("#cartItemCount");
    const totalPrice = $("#totalPrice");
    const userId = $("#userId").val();

    // Fetch the cart item count when the page loads
    fetchCartItemCount();

    // Event: Clicking the cart icon opens the modal and fetches cart items
    cartIcon.on("click", function () {
        openCartModal();
        fetchCartItems();
    });

    // Open and close modal functions
    function openCartModal() {
        $("#shoppingCartModal").addClass("show");
    }

    window.closeCartModal = function () {
        $("#shoppingCartModal").removeClass("show");
        $("body").removeClass("modal-open"); // Allow background scroll
    };

    // Close modal when clicking outside the modal content
    $(document).on("mousedown", function (event) {
        // Only close if modal is open and click is outside modal content
        if ($("#shoppingCartModal").hasClass("show") && !$(event.target).closest(".modal-content").length) {
            window.closeCartModal();
        }
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
                    updateTotalPrice(response.items);
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
        let html = `<div class="cart-item-list">`;
        items.forEach(item => {
            html += `
                <div class="cart-item">
                    <span class="cart-item-name">Menu Item ID: ${item.menuItemId}</span>
                    <div class="quantity-control">
                        <button onclick="decreaseQuantity(${item.menuItemId})">-</button>
                        <span>${item.quantity}</span>
                        <button onclick="increaseQuantity(${item.menuItemId})">+</button>
                        <button onclick="removeCartItem(${item.menuItemId})"><i class="bi bi-trash"></i></button>
                    </div>
                    <span class="cart-item-price">$${(item.price * item.quantity).toFixed(2)}</span>
                </div>`;
        });
        html += `</div>`;
        cartItemsContainer.html(html);
    }

    // Update total price display
    function updateTotalPrice(items) {
        const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        totalPrice.text(`Total: $${total.toFixed(2)}`);
        cartItemCount.text(items.length); // Update cart item count badge
    }

    // Handle removing an item from the cart
    window.removeCartItem = function (itemId) {
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
    };

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
        totalPrice.text("Total: $0.00");
        cartItemCount.text(0); // Reset badge count
    }
});
