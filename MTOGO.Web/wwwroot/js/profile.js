function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

document.addEventListener("DOMContentLoaded", function () {
    const userId = document.getElementById("userId").value;
    const tabs = document.querySelectorAll(".tab-link");
    const contents = document.querySelectorAll(".tab-content");

    tabs.forEach(tab => {
        tab.addEventListener("click", function (e) {
            e.preventDefault();
            tabs.forEach(t => t.classList.remove("active"));
            contents.forEach(c => c.style.display = "none");
            this.classList.add("active");
            document.getElementById(this.getAttribute("data-tab")).style.display = "block";
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
                    console.log("Profile updated successfully with a new token.");
                }
                location.reload();
            } else {
                alert(`Error: ${data.message}`);
                console.error("Update error:", data.message);
            }
        } catch (error) {
            console.error("Error updating profile:", error);
            alert("An error occurred while updating the profile.");
        }
    });
    // Delete Account Action
    document.getElementById("deleteAccountBtn").addEventListener("click", function () {
        // Show the confirmation modal for delete
        $('#deleteModal').modal('show');
    });

    // Confirm Delete Account
    document.getElementById("confirmDeleteBtn").addEventListener("click", async function () {
        const userId = document.getElementById("userId").value;

        try {
            const response = await fetch(`/Auth/DeleteAccount?userId=${userId}`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });

            if (response.ok) {
                // Redirect after successful deletion
                window.location.href = "/";
            } else {
                // Handle error if account deletion fails
                const errorData = await response.json();
                alert("Error deleting account: " + errorData.message);
            }
        } catch (error) {
            console.error("Error deleting account:", error);
            alert("An error occurred. Please try again.");
        }

        // Close modal after deletion
        $('#deleteModal').modal('hide');
    });

    // Log Out Action
    document.getElementById("logoutBtn").addEventListener("click", function () {
        // Log out confirmation

            // Make a fetch request to the Logout action in your AuthController
            fetch('/Auth/Logout', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {
                    if (response.ok) {
                        // Redirect to home after logout
                        location.href = '/Home/Index';
                    } else {
                        alert("Logout failed. Please try again.");
                    }
                })
                .catch(error => {
                    console.error("Error logging out:", error);
                    alert("An error occurred. Please try again.");
                });
        });
});
