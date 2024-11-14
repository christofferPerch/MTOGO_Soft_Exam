function toggleCart() {
    const cartDrawer = document.getElementById("shoppingCartDrawer");
    if (cartDrawer) {
        cartDrawer.style.display = cartDrawer.style.display === "none" ? "block" : "none";

        // If the cart is being opened, load the cart items
        if (cartDrawer.style.display === "block") {
            loadCartItems();
        }
    } else {
        console.error("Shopping cart drawer not found in the DOM.");
    }
}

document.addEventListener("DOMContentLoaded", function () {
    const cartButton = document.getElementById("cartButton");

    // Check if the cart button exists in the DOM
    if (cartButton) {
        console.log("Cart button found.");
        cartButton.addEventListener("click", function (event) {
            event.preventDefault();
            toggleCart();
        });
    } else {
        console.warn("Cart button not found in the DOM.");
    }

    // Load the cart item count on page load
    updateCartItemCount();
});

async function loadCartItems() {
    const userIdElement = document.getElementById("userId");
    if (!userIdElement) {
        console.error("User ID element not found.");
        return;
    }

    const userId = userIdElement.value;
    const cartItemsContainer = document.getElementById("shoppingCartItems");
    const emptyCartMessage = document.getElementById("emptyCartMessage");

    try {
        const response = await fetch(`/ShoppingCart/${userId}`);

        if (!response.ok) {
            throw new Error(`Failed to fetch cart items: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();

        // Clear existing items
        cartItemsContainer.innerHTML = "";

        if (data && data.isSuccess && data.result && data.result.items.length > 0) {
            emptyCartMessage.style.display = "none";
            data.result.items.forEach(item => {
                const itemHtml = `
                    <div class="cart-item">
                        <img src="${item.imageUrl || 'https://via.placeholder.com/50'}" alt="${item.name}" class="item-image">
                        <div class="item-details">
                            <div class="item-name">${item.name}</div>
                            <div class="item-price">${item.price.toFixed(2)} kr.</div>
                        </div>
                        <div class="item-quantity">
                            <input type="number" value="${item.quantity}" min="1" class="quantity-input" data-menuitemid="${item.menuItemId}">
                        </div>
                    </div>
                `;
                cartItemsContainer.insertAdjacentHTML("beforeend", itemHtml);
            });
        } else {
            emptyCartMessage.style.display = "block";
            cartItemsContainer.innerHTML = "<p>Your cart is empty.</p>";
        }
    } catch (error) {
        console.error("Error loading cart items:", error);
        cartItemsContainer.innerHTML = "<p>Failed to load cart items.</p>";
    }
}

async function updateCartItemCount() {
    const userIdElement = document.getElementById("userId");
    if (!userIdElement) {
        console.error("User ID element not found.");
        return;
    }

    const userId = userIdElement.value;
    const cartItemCountElement = document.getElementById("cartItemCount");

    if (!cartItemCountElement) {
        console.error("Cart item count element not found.");
        return;
    }

    try {
        const response = await fetch(`/ShoppingCart/${userId}`);

        if (!response.ok) {
            throw new Error(`Failed to fetch cart item count: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();

        if (data && data.isSuccess && data.result) {
            const itemCount = data.result.items.reduce((count, item) => count + item.quantity, 0);
            cartItemCountElement.textContent = itemCount;
        } else {
            cartItemCountElement.textContent = "0";
        }
    } catch (error) {
        console.error("Error updating cart item count:", error);
        cartItemCountElement.textContent = "0";
    }
}
