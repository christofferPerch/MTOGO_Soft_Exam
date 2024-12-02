document.addEventListener("DOMContentLoaded", function () {
    const foodCategoryDropdown = document.getElementById("foodCategory");
    const selectedCategory = foodCategoryDropdown.getAttribute("data-selected-category");

    if (selectedCategory) {
        foodCategoryDropdown.value = selectedCategory;
    }
});
