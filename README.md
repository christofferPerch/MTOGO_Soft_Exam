# README: Development of Large Systems

This README serves as a structured appendix to the **Development of Large Systems** report for the MTOGO Food Delivery System.

## Contents of the README

- **User Stories**: A comprehensive collection of user stories, capturing the requirements and expectations of stakeholders. This includes customers, administrators, restaurant owners, delivery agents, and the system itself. The user stories provided serve as the foundation for our development process, aligning technical implementation with stakeholder goals.
  
- **Acceptance Criteria**: Detailed examples of user stories with acceptance criteria are included to illustrate how we defined and verified functionality. These examples guided our testing and validation processes.

- **EER Diagrams**: Entity-Relationship diagrams for key services, detailing the database design and relationships for authentication, order management, payments, restaurant operations, and reviews.

This README reflects our commitment to documenting the development process and providing a clear reference for understanding the system's requirements, architecture, and implementation.



# User Stories

In our project, we utilized user stories to capture the requirements and expectations of the system’s stakeholders. These stories form the foundation of our development process, focusing on delivering value to customers, administrators, restaurant owners, delivery agents, and the system itself.

## User Story 1
**As a Customer, I want to register on the website, so that I can create an account to place orders and track my activities.**

## User Story 2
**As a Customer, I want to log in to the website, so that I can access my account, create orders and view my orders.**

## User Story 3
**As a Customer, I want to update my profile, so that I can keep it up to date with my correct information and address.**

## User Story 4
**As a Customer, I want to delete my profile, so that my information is removed from the system.**

## User Story 5
**As an Admin, I want to be able to create a restaurant and assign a restaurant owner to it, so that they can manage their restaurant.**

## User Story 6
**As a Customer, I want to search for restaurants by zip code or food category, so that I can find options near me or with specific cuisine types.**

## User Story 7
**As a Restaurant Owner, I want to add menu items to my restaurant’s page, so that customers can view and order the food we offer.**

## User Story 8
**As a Restaurant Owner, I want to delete menu items from my restaurant’s page, so that outdated or unavailable items are not shown to customers.**

## User Story 9
**As a Restaurant Owner, I want to update existing menu items, so that customers have accurate information on the menu.**

## User Story 10
**As a Restaurant Owner, I want to update my restaurant’s details (such as hours, location, and contact information), so that customers have the latest information.**

## User Story 11
**As a Customer, I want to add menu items from a restaurant to my shopping cart, so that it becomes a part of my final order.**

## User Story 12
**As a Customer, I want to be able to view my shopping cart, so that I can see what is in it, before I place the order.**

## User Story 13
**As a Customer, I want to update items in my shopping cart, so that I can adjust my order before checking out.**

## User Story 14
**As a Customer, I want to remove items from my shopping cart, so that I can adjust my order as needed.**

## User Story 15
**As a Customer, I want to pay for items in my shopping cart, so that an order is created and processed for delivery.**

## User Story 16
**As a Customer, I want to view the status of my active order, so that I know when to expect delivery.**

## User Story 17
**As a Customer, I want to view my previous orders, so that I can review past purchases.**

## User Story 18
**As a Customer, I want to leave feedback on the restaurant with a rating and comment, so that other customers in the future know what to expect.**

## User Story 19
**As a Customer, I want to be able to remove or update my reviews on a restaurant, so that the reviews are always up to date.**

## User Story 20
**As a Customer, I want to view the reviews on a specific restaurant, so that I know what to expect of the restaurant.**

## User Story 21
**As a System, when a customer places an order, I want to send them an order confirmation email, so that they verify their order.**

## User Story 22
**As an Admin, I want to be able to set up an account for a delivery agent, so that they can start delivering orders.**

## User Story 23
**As a Customer, I want to choose my preferred language (English or Danish) during registration, so that I can use the website in my preferred language.**

## User Story 24
**As a Customer, I want to receive an email or SMS notification after successful registration, so that I am informed that my account was created.**

## User Story 25
**As a Restaurant Owner, I want to view the total fees paid to MTOGO for my orders, so that I can track my expenses for using the service.**

## User Story 26
**As a Restaurant Owner, I want to view the breakdown of fees per order, so that I understand how the variable fees are calculated based on the order value.**

## User Story 27
**As a Customer, I want to receive SMS or app updates as my order status changes, so that I am informed of the progress of my delivery.**

## User Story 28
**As an Admin, I want to define bonus rules for delivery agents (based on order value, working hours, and customer reviews), so that bonuses are automatically calculated.**

## User Story 29
**As a Delivery Agent, I want to view my bonus details for completed deliveries, so that I know how my earnings were calculated.**

## User Story 30
**As a System, I want to call the external payment processing service when a customer places an order, so that the payment is processed securely.**

## User Story 31
**As a Customer, I want to leave a review for the delivery agent in addition to the restaurant, so that I can provide feedback on their service.**

## User Story 32
**As a System, I want to send a feedback request to the customer after order delivery, so that they can rate the service.**

## User Story 33
**As a Manager, I want to view the total number of open orders in real-time, so that I can monitor the system's performance.**

## User Story 34
**As a Manager, I want to see average orders placed over a 24-hour period, so that I can analyze customer activity trends.**

## User Story 35
**As a Manager, I want to view the average processing time for orders, so that I can assess system efficiency.**

## User Story 36
**As a System, I want to send an SMS or app notification to the customer when their order is picked up by the delivery agent, so that they know the delivery is en route.**

## User Story 37
**As a Customer, I want to be able to choose between receiving order updates via SMS or the app, so that I can receive notifications in my preferred way.**

## User Story 38
**As an Admin, I want to onboard a new restaurant owner with an account and access permissions, so that they can manage their restaurant details and orders.**

## User Story 39
**As a System, I want to be able to handle an increase in the number of customers and orders, so that the service remains reliable as the user base grows.**

## User Story 40
**As a System, I want to support adding a complaints module in the future, so that customer and supplier complaints can be managed efficiently.**


Below are examples of user stories from our project with detailed acceptance criteria:

## Examples with Detailed Acceptance Criteria

### **User Story 1**  
**As a Customer, I want to register on the website, so that I can create an account to place orders and track my activities.**

**Acceptance Criteria:**  
- **Scenario: Successful registration on the website**  
  Given that I am on the registration page  
  When I enter all required information (full name, email address, phone number, address, password, and confirm password) with valid data  
  Then my account is created and I am assigned the "Customer" role by default, so that I have appropriate access and permissions for ordering.  

- **Scenario: Registration with existing email**  
  Given that I attempt to register with an email already in use  
  When I submit the registration form  
  Then I see an error message stating that the email is already registered and my account is not created.  

- **Scenario: Password validation failure**  
  Given that I enter a password not meeting the complexity requirements  
  When I submit the registration form  
  Then I see an error message explaining the password requirements.  
  _Complexity requirements: Minimum 8 characters, at least one uppercase letter, one lowercase letter, one numeric digit, and one special character._  

---

### **User Story 2**  
**As a Customer, I want to log in to the website, so that I can access my account and view my orders.**

**Acceptance Criteria:**  
- **Scenario: Successful login**  
  Given that I have a registered account and am on the login page  
  When I enter my valid email and password  
  Then I am logged in, and I am redirected to my account dashboard.  

- **Scenario: Invalid login credentials**  
  Given that I enter an incorrect email or password  
  When I attempt to log in  
  Then I see an error message indicating "Invalid credentials."  

- **Scenario: Password recovery (not implemented yet)**  
  Given that I have forgotten my password and am on the login page  
  When I click on "Forgot Password" and provide my registered email  
  Then I receive an email with instructions to reset my password.  

- **Scenario: Account lock after multiple failed login attempts (not implemented yet)**  
  Given that I enter incorrect login credentials multiple times (e.g., 5 failed attempts)  
  When I attempt another login  
  Then my account is temporarily locked, and I see a message indicating "Too many failed attempts. Your account has been locked. Please try again later or reset your password."  

---

### **User Story 6**  
**As a Customer, I want to search for restaurants by zip code or food category, so that I can find options near me or with specific cuisine types.**

**Acceptance Criteria:**  
- **Scenario: Successful restaurant search by zip code**  
  Given that I am on the restaurant search page  
  When I enter a valid zip code and click search  
  Then I see a list of restaurants near that zip code with details such as name, location, and average rating.  

- **Scenario: Successful restaurant search by food category**  
  Given that I am on the restaurant search page  
  When I select one or more food categories (e.g., Italian, Vegan) and click search  
  Then I see a list of restaurants that match the selected categories.  

- **Scenario: Combined search by zip code and food category**  
  Given that I am on the restaurant search page  
  When I enter a valid zip code and select one or more food categories  
  Then I see a list of restaurants near that zip code that match the selected food categories.  

- **Scenario: Invalid zip code entry**  
  Given that I enter an invalid zip code format in the search bar  
  When I attempt to search  
  Then I see an error message instructing me to enter a valid zip code.  

- **Scenario: No search results**  
  Given that I search for restaurants with criteria that don’t match any available restaurants  
  When I click search  
  Then I see a message stating "No results found for your search criteria."  

---

### **User Story 15**  
**As a Customer, I want to pay for items in my shopping cart, so that an order is created and processed for delivery.**

**Acceptance Criteria:**  
- **Scenario: Successful payment**  
  Given that I have items in my shopping cart  
  When I enter valid payment details and click “Pay Now”  
  Then I see a confirmation message that my payment was successful, and an order is created with a status of “Processing.”  

- **Scenario: Failed payment due to invalid details**  
  Given that I have items in my shopping cart  
  When I enter invalid payment details and click “Pay Now”  
  Then I see an error message and am prompted to re-enter my payment details.  

- **Scenario: External service failure**  
  Given that I have items in my shopping cart  
  When I attempt to pay and the payment service is unavailable  
  Then I see an error message stating the service is temporarily unavailable.  

- **Scenario: Empty cart prevention**  
  Given that my shopping cart is empty  
  When I attempt to proceed to payment  
  Then I see a message stating that I cannot proceed without adding items to my cart.  

---

### **User Story 21**  
**As a System, when a customer places an order, I want to send them an order confirmation email, so that they verify their order.**

**Acceptance Criteria:**  
- **Scenario: Successful order confirmation email**  
  Given that a customer has successfully placed an order  
  When the system processes the order  
  Then an order confirmation email is sent to the customer’s registered email address and the email contains order details and a unique order ID.  

- **Scenario: Failed email delivery**  
  Given that a customer has placed an order  
  When the email service is unavailable  
  Then the system logs the failure and retries sending the email after a defined interval.  

- **Scenario: Invalid email address**  
  Given that a customer has placed an order  
  When the system detects an invalid or unverified email address  
  Then the system logs the issue, and the customer is notified in their account to update their email information.  

---
### **EER Diagrams**  

**Auth Service:**

  ![EER Diagram](Documentation/Diagrams/AuthServiceEERDiagram.png)

  **OrderServiceDB:**

  ![Domain Diagram](Documentation/Diagrams/OrderServiceEERDiagram.png)

  **PaymentDB:**

  ![Domain Diagram](Documentation/Diagrams/PaymentEERDiagram.png)

  **RestaurantServiceDB:** 

  ![Domain Diagram](Documentation/Diagrams/RestaurantEERDiagram.png)

  **ReviewServiceDB:**

  ![Domain Diagram](Documentation/Diagrams/ReviewEERDiagram.png)

