function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

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


document.addEventListener("DOMContentLoaded", function () {
    const tabs = document.querySelectorAll(".tab-link");
    const contents = document.querySelectorAll(".tab-content");

    contents.forEach(content => content.style.display = "none");

    const urlParams = new URLSearchParams(window.location.search);
    const activeTab = urlParams.get("tab") || "personal-info"; 
    const activeTabElement = document.querySelector(`.tab-link[data-tab="${activeTab}"]`);
    const activeContentElement = document.getElementById(activeTab);

    if (activeTabElement && activeContentElement) {
        activeTabElement.classList.add("active");
        activeContentElement.style.display = "block";
    }

    tabs.forEach(tab => {
        tab.addEventListener("click", function (e) {
            e.preventDefault();

            tabs.forEach(t => t.classList.remove("active"));
            contents.forEach(c => c.style.display = "none");

            this.classList.add("active");
            document.getElementById(this.getAttribute("data-tab")).style.display = "block";

            const newUrl = `${window.location.pathname}?tab=${this.getAttribute("data-tab")}`;
            window.history.replaceState(null, "", newUrl);
        });
    });


    const clickableValues = document.querySelectorAll(".setting-value.clickable");
    clickableValues.forEach(value => {
        value.addEventListener("click", function () {
            const field = this.getAttribute("data-field");
            const currentValue = this.textContent.trim();
            document.getElementById("editModalLabel").textContent = `Edit ${field.charAt(0).toUpperCase() + field.slice(1)}`;
            document.getElementById("editInput").value = currentValue;
            document.getElementById("saveChangesBtn").setAttribute("data-field", field);
            new bootstrap.Modal(document.getElementById("editModal")).show();
        });
    });

    document.getElementById("saveChangesBtn").addEventListener("click", async function () {
        const field = this.getAttribute("data-field");
        const newValue = document.getElementById("editInput").value.trim();

        const payload = {};
        payload[field] = newValue;

        const token = getCookie('JWTToken');
        const headers = {
            'Content-Type': 'application/json'
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`/Auth/UpdateProfile`, {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(payload)
            });

            const data = await response.json();

            if (data.success) {
                if (data.token) {
                    document.cookie = `JWTToken=${data.token}; path=/;`;
                }

                const fieldElement = document.querySelector(`.setting-value[data-field="${field}"]`);
                if (fieldElement) {
                    fieldElement.textContent = newValue;
                }

                displayMessage("Profile updated successfully!", true);
                const editModal = document.getElementById("editModal");
                const modalInstance = bootstrap.Modal.getInstance(editModal);
                if (modalInstance) {
                    modalInstance.hide();
                }
            } else {
                displayMessage(`Error: ${data.message}`, false);
            }
        } catch (error) {
            console.error("Error updating profile:", error);
            displayMessage("An error occurred while updating the profile.", false);
        }
    });

    document.getElementById("deleteAccountBtn").addEventListener("click", function () {
        $('#deleteModal').modal('show');
    });

    document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
        const userId = document.getElementById("userId").value;

        try {
            const response = await fetch(`/Auth/DeleteAccount?userId=${userId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                displayMessage("Account deleted successfully.", true);
                window.location.href = "/";
            } else {
                const errorData = await response.json();
                displayMessage("Error deleting account: " + errorData.message, false);
            }
        } catch (error) {
            console.error("Error deleting account:", error);
            displayMessage("An error occurred. Please try again.", false);
        }

        $('#deleteModal').modal('hide');
    });

    document.getElementById("logoutBtn").addEventListener("click", function () {
        fetch('/Auth/Logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (response.ok) {
                    displayMessage("Logout successful.", true);
                    location.href = '/Home/Index';
                } else {
                    displayMessage("Logout failed. Please try again.", false);
                }
            })
            .catch(error => {
                console.error("Error logging out:", error);
                displayMessage("An error occurred. Please try again.", false);
            });
    });
});
