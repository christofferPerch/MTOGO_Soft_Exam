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

document.addEventListener("DOMContentLoaded", () => {
    const reviewsPerPage = 5; 
    let currentPage = 1;

    const reviewsContainer = document.getElementById("reviewsContainer");
    const prevPageBtn = document.getElementById("prevPageBtn");
    const nextPageBtn = document.getElementById("nextPageBtn");
    const submitReviewBtn = document.getElementById("submitReviewBtn");

    const restaurantId = document.getElementById("restaurantId").value; 
    const customerId = document.getElementById("userId").value; 

    const renderReviews = () => {
        const sortedReviews = [...reviews].sort((a, b) => new Date(b.ReviewTimestamp) - new Date(a.ReviewTimestamp));

        const startIndex = (currentPage - 1) * reviewsPerPage;
        const endIndex = startIndex + reviewsPerPage;

        const paginatedReviews = sortedReviews.slice(startIndex, endIndex);

        reviewsContainer.innerHTML = "";

        paginatedReviews.forEach((review) => {
            const reviewHtml = `
            <div class="review-card">
                <strong>Rating:</strong> ${review.FoodRating}/5<br />
                <strong>Comment:</strong> ${review.Comments || "No comment provided"}<br />
                <small class="text-muted">Reviewed on: ${new Date(review.ReviewTimestamp).toLocaleDateString()}</small>
            </div>
        `;
            reviewsContainer.innerHTML += reviewHtml;
        });

        pageInfo.textContent = `Page ${currentPage}`;
        prevPageBtn.disabled = currentPage === 1;
        nextPageBtn.disabled = endIndex >= sortedReviews.length;
    };

    const submitReview = async () => {
        const foodRating = document.getElementById("foodRating").value;
        const comments = document.getElementById("comments").value;

        const reviewData = {
            RestaurantId: parseInt(restaurantId),
            CustomerId: customerId,
            FoodRating: parseInt(foodRating),
            Comments: comments
        };

        try {
            const response = await fetch('/Restaurant/AddReview', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(reviewData)
            });

            const result = await response.json();

            if (response.ok && result.success) {
                displayMessage("Review added successfully!", true);
                reviews.push({
                    ...reviewData,
                    ReviewTimestamp: new Date().toISOString(),
                    Id: result.result 
                });
                currentPage = Math.ceil(reviews.length / reviewsPerPage);
                renderReviews();
                document.getElementById("addReviewForm").reset();
                const addReviewModal = bootstrap.Modal.getInstance(document.getElementById("addReviewModal"));
                if (addReviewModal) addReviewModal.hide();
            } else {
                displayMessage("Failed to add review: " + result.message, false);
            }
        } catch (error) {
            console.error("Error adding review:", error);
            displayMessage("An error occurred while adding the review.", false);
        }
    };

    submitReviewBtn.addEventListener("click", submitReview);

    prevPageBtn.addEventListener("click", () => {
        if (currentPage > 1) {
            currentPage--;
            renderReviews();
        }
    });

    nextPageBtn.addEventListener("click", () => {
        if ((currentPage * reviewsPerPage) < reviews.length) {
            currentPage++;
            renderReviews();
        }
    });

    renderReviews();
});
