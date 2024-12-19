$(document).ready(function () {
    const cartIcon = $("#cartIcon");
    const cartItemsContainer = $("#cartItemsContainer");
    const cartItemCount = $("#cartItemCount");
    const totalPrice = $("#totalPrice");
    const userId = $("#userId").val();

    fetchCartItemCount();

    cartIcon.on("click", function () {
        openCartModal();
        fetchCartItems();
    });

    function openCartModal() {
        $("#shoppingCartModal").addClass("show");
    }

    window.closeCartModal = function () {
        $("#shoppingCartModal").removeClass("show");
        $("body").removeClass("modal-open"); 
    };

    $(document).on("mousedown", function (event) {
        if ($("#shoppingCartModal").hasClass("show") && !$(event.target).closest(".modal-content").length) {
            window.closeCartModal();
        }
    });

    function fetchCartItems() {
        if (!userId) {
            console.error("User ID is missing.");
            return;
        }

        $.ajax({
            url: `/ShoppingCart/GetCart?userId=${userId}`,
            method: "GET",
            success: async function (response) {
                console.log("Fetched cart items:", response);
                if (response.items && response.items.length > 0) {
                    const enrichedItems = await enrichCartItems(response.items);
                    populateCart(enrichedItems);
                    updateTotalPrice(response.items);
                } else {
                    handleEmptyCart();
                }
            },
            error: function (err) {
                console.error("Error fetching cart items:", err);
                handleEmptyCart(); 
            }
        });
    }

    async function enrichCartItems(items) {
        const promises = items.map(async (item) => {
            try {
                const result = await $.ajax({
                    url: `/Restaurant/GetCartDetails`,
                    method: "GET",
                    data: {
                        restaurantId: item.restaurantId,
                        menuItemId: item.menuItemId,
                    },
                });
                if (result && result.name) {
                    item.name = result.name;
                    item.image = result.image || null; 
                } else {
                    item.name = "Unknown Item";
                }
            } catch (error) {
                console.error(`Error fetching details for MenuItemId: ${item.menuItemId}`, error);
                item.name = "Unknown Item";
            }
            return item;
        });

        return Promise.all(promises);
    }

    function populateCart(items) {
        let html = `<div class="cart-item-list">`;
        items.forEach((item) => {
            html += `
            <div class="cart-item d-flex align-items-center mb-3">
                <!-- Image -->
                <div class="cart-item-image me-3">
                    <img src="/images/restaurant.jpg" alt="${item.name}" style="width: 50px; height: 50px; object-fit: cover; border-radius: 4px;">
                </div>
                <!-- Name and Quantity Controls -->
                <div class="cart-item-details flex-grow-1">
                    <span class="cart-item-name fw-bold">${item.name}</span>
                    <div class="quantity-control mt-2">
                        <button onclick="decreaseQuantity('${userId}', ${item.menuItemId})" class="btn btn-sm btn-secondary me-1">-</button>
                        <span id="quantity-${item.menuItemId}" class="px-2">${item.quantity}</span>
                        <button onclick="increaseQuantity('${userId}', ${item.menuItemId})" class="btn btn-sm btn-secondary ms-1">+</button>
                        <button onclick="removeCartItem('${userId}', ${item.menuItemId})" class="btn btn-sm btn-danger ms-3">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
                <!-- Price -->
                <div class="cart-item-price text-end ms-3">
                    <span>$${(item.price * item.quantity).toFixed(2)}</span>
                </div>
            </div>`;
        });
        html += `</div>`;
        cartItemsContainer.html(html);
    }

    function updateTotalPrice(items) {
        const total = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
        totalPrice.text(`Total: $${total.toFixed(2)}`);
        cartItemCount.text(items.length); 
    }

    window.removeCartItem = function (userId, menuItemId) {
        const cart = {
            userId: userId,
            items: [{ menuItemId: menuItemId, quantity: 0 }], 
        };

        $.ajax({
            url: "/ShoppingCart/SetCart",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(cart),
            success: function () {
                console.log(`Item with MenuItemId ${menuItemId} removed successfully.`);
                fetchCartItems(); 
                fetchCartItemCount(); 
            },
            error: function (err) {
                console.error("Error removing cart item:", err);
            },
        });
    };

    window.increaseQuantity = function (userId, menuItemId) {
        const currentQuantity = parseInt($(`#quantity-${menuItemId}`).text(), 10);
        const cart = {
            userId: userId,
            items: [{ menuItemId: menuItemId, quantity: currentQuantity + 1 }],
        };

        $.ajax({
            url: "/ShoppingCart/SetCart",
            method: "POST",
            contentType: "application/json",
            data: JSON.stringify(cart),
            success: function () {
                console.log(`Quantity increased for MenuItemId: ${menuItemId}`);
                fetchCartItems(); 
            },
            error: function (err) {
                console.error("Error increasing quantity:", err);
            },
        });
    };

    window.decreaseQuantity = function (userId, menuItemId) {
        const currentQuantity = parseInt($(`#quantity-${menuItemId}`).text(), 10);
        if (currentQuantity > 1) {
            const cart = {
                userId: userId,
                items: [{ menuItemId: menuItemId, quantity: currentQuantity - 1 }],
            };

            $.ajax({
                url: "/ShoppingCart/SetCart",
                method: "POST",
                contentType: "application/json",
                data: JSON.stringify(cart),
                success: function () {
                    console.log(`Quantity decreased for MenuItemId: ${menuItemId}`);
                    fetchCartItems(); 
                },
                error: function (err) {
                    console.error("Error decreasing quantity:", err);
                },
            });
        } else {
            removeCartItem(userId, menuItemId);
        }
    };

    function fetchCartItemCount() {
        if (!userId) {
            console.error("User ID is missing.");
            cartItemCount.text(0); 
            return;
        }

        $.ajax({
            url: `/ShoppingCart/GetCart?userId=${userId}`,
            method: "GET",
            success: function (response) {
                if (response.items && response.items.length > 0) {
                    cartItemCount.text(response.items.length);
                } else {
                    cartItemCount.text(0); 
                }
            },
            error: function (err) {
                console.error("Error fetching cart item count:", err);
                cartItemCount.text(0); 
            },
        });
    }

    function handleEmptyCart() {
        cartItemsContainer.html("<p>Your cart is empty.</p>");
        totalPrice.text("Total: $0.00");
        cartItemCount.text(0); 
    }
});
