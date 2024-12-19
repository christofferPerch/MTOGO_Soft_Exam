function displayMessage(message, isSuccess) {
    console.log("Displaying message:", message);
    const alertType = isSuccess ? 'alert-success' : 'alert-danger';
    const messageHtml = `<div class="alert ${alertType}">${message}</div>`;
    document.getElementById("messageContainer").innerHTML = messageHtml;
    document.getElementById("messageContainer").style.display = 'block';

    setTimeout(function () {
        document.getElementById("messageContainer").style.display = 'none';
    }, 3000);
}

document.addEventListener('DOMContentLoaded', function () {
    const userId = document.getElementById('userId').value; 
    const cartItemCountElement = document.getElementById('cartItemCount'); 

    document.querySelectorAll('.add-to-cart').forEach(function (button) {
        button.addEventListener('click', function () {
            const menuItemId = this.getAttribute('data-id');
            const restaurantId = this.getAttribute('data-restaurant-id');
            const price = this.getAttribute('data-price');
            const name = this.getAttribute('data-name');
            const description = this.getAttribute('data-description');

            document.getElementById('menuItemId').value = menuItemId;
            document.getElementById('restaurantId').value = restaurantId;
            document.getElementById('price').value = price;
            document.getElementById('menuItemName').value = name; 

            document.getElementById('menuItemNameDisplay').textContent = name; 
            document.getElementById('menuItemDescription').textContent = description;
            document.getElementById('menuItemPrice').textContent = `$${parseFloat(price).toFixed(2)}`;

            const quantityModal = new bootstrap.Modal(document.getElementById('quantityModal'));
            quantityModal.show();
        });
    });
   
    document.getElementById('confirmAddToCart').addEventListener('click', function () {
        const menuItemId = document.getElementById('menuItemId').value;
        const restaurantId = document.getElementById('restaurantId').value;
        const price = document.getElementById('price').value;
        const quantity = document.getElementById('quantity').value;
        const name = document.getElementById('menuItemName').value; 


        if (quantity < 1) {
            displayMessage('Please enter a valid quantity.', false);
            return;
        }

        const cart = {
            userId: userId,
            items: [
                {
                    restaurantId: parseInt(restaurantId, 10),
                    menuItemId: parseInt(menuItemId, 10),
                    quantity: parseInt(quantity, 10),
                    price: parseFloat(price),
                    name: name,
                },
            ],
        };

        fetch('/ShoppingCart/SetCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(cart),
        })
            .then((response) => {
                if (response.ok) {
                    displayMessage('Item added to cart successfully!', true);
                    fetchCartItemCount();
                } else {
                    response.json().then(data => {
                        displayMessage(data.message || 'Error adding item to cart.', false);
                    });
                }
                bootstrap.Modal.getInstance(document.getElementById('quantityModal')).hide();
            })
            .catch((error) => {
                console.error('Error:', error);
                displayMessage('An error occurred while adding the item to the cart.', false);
            });
    });

    function fetchCartItemCount() {
        if (!userId) {
            console.error("User ID is missing.");
            cartItemCountElement.textContent = 0; 
            return;
        }

        $.ajax({
            url: `/ShoppingCart/GetCart?userId=${userId}`,
            method: "GET",
            success: function (response) {
                if (response.items && response.items.length > 0) {
                    cartItemCountElement.textContent = response.items.length; 
                } else {
                    cartItemCountElement.textContent = 0; 
                }
            },
            error: function (err) {
                console.error("Error fetching cart item count", err);
                displayMessage('Failed to fetch cart item count.', false);
                cartItemCountElement.textContent = 0; 
            }
        });
    }
});
