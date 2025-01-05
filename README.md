# **MTOGO \- Software Exam Project**

### **How to run our project**

**Docker Compose:**  
Have Docker installed on your desktop and right click on the docker-compose in the project and select "Compose Up". You might have to clean the solution and build it first if any errors occour. 

**Run without Docker:**

1. Clone our repository “git clone [https://github.com/christofferPerch/MTOGO\_Soft\_Exam.git](https://github.com/christofferPerch/MTOGO_Soft_Exam.git)”  
     
2. Import the BACPAC files located [HERE](./Documentation/BacpacFiles/) containing the MSSQL databases into your SQL server.   
     
3. Check appsettings.development.json files in each microservice project and ensure the connection string for the MSSQL database is using the correct server.   
     
4. Setup Redis through docker: docker run \-d \--name redis-stack-server \-p 6379:6379 redis/redis-stack-server:latest  
     
5. Setup RabbitMQ through docker: docker run \-d \--name rabbitmq \-p 5672:5672 \-p 15672:15672 rabbitmq:3-management  
     
6. Configure Startup Projects in Visual Studio make sure it uses Multiple startup projects and starts everything except the MTOGO.MessageBus, MTOGO.DataAccess and MTOGO.UnitTests projects.   
     
7. It should now open the API Gateway and Web Project when running the application.

---
# Test Strategy for MTOGO Project

## 1. Testing Scope
The MTOGO project delivers a scalable and reliable food delivery platform built using a microservices architecture. The system encompasses services such as `OrderService`, `AuthService`, `RestaurantService`, `ReviewService`, and `EmailService`. This strategy outlines the testing approaches for validating core functionalities, ensuring seamless inter-service communication, and meeting performance and quality benchmarks.

To ensure that our code is not only functional but also adheres to high-quality standards, we will apply the **SQALE** (Software Quality Assessment based on Lifecycle Expectations) Method. This approach provides a structured framework for assessing key aspects of software quality throughout its lifecycle. Our focus will include:

- **Changeability:** Ensuring the codebase can be updated or modified efficiently when changes are required.
- **Maintainability:** Minimizing the effort needed to identify and resolve bugs, ensuring the system remains stable and reliable over time.
- **Portability:** Enhancing the system’s adaptability, allowing it to run across different platforms or environments with minimal modifications.
- **Reusability:** Designing code components that can be leveraged across multiple features or projects to reduce duplication and improve development efficiency.
- **Technical Debt:** Monitoring and addressing areas that may require additional effort to improve or refactor, with a proactive focus on preventing long-term maintenance challenges.

## 2. Types of Tests

### **Unit Tests**
- **Scope:** Validate individual functions, classes, and methods in isolation (e.g., order validation, payment processing, and delivery status updates).
- **Objective:** Ensure each component behaves as expected independently, focusing on key logic such as order creation and error handling.

### **Integration Tests**
- **Scope:** Assess interactions between services, such as communication between `OrderService`, `PaymentService`, and `DeliveryService`.
- **Objective:** Confirm that services communicate correctly, including message exchanges via RabbitMQ and interactions with external APIs and databases.

### **Acceptance Tests**
- **Scope:** Verify end-to-end workflows to ensure the application meets user expectations for functionality and usability.
- **Objective:** Ensure that essential workflows, such as order submission and notifications, function correctly from a user’s perspective.

### **System Tests**
- **Scope:** Validate complete user workflows, such as registration, login, and HTTP status code handling.
- **Objective:** Confirm that the system performs as expected across all integrated components and user scenarios.

### **Mutation Tests**
- **Scope:** Evaluate the effectiveness of the test suite by introducing small, deliberate code mutations.
- **Objective:** Detect weaknesses in test cases and improve coverage to ensure they catch code changes and potential errors.

### **Performance and Load Tests**
- **Scope:** Measure the system’s performance, stability, and scalability under expected user growth (e.g., scaling from 300,000 to 1.5 million users and up to 18 million orders).
- **Objective:** Ensure that the application maintains responsiveness and reliability during peak loads.

### **Regression Tests**
- **Scope:** Continuously validate existing features to ensure that new changes do not introduce defects.
- **Implementation:** Automated regression tests run in the CI/CD pipeline to quickly detect and prevent regressions.

## 3. Test Coverage Goals
- A minimum **70% overall code coverage** is set, with an emphasis on business-critical areas such as order placement, payment processing, and notifications.
- Track mutation score improvements to identify and close coverage gaps.
- Use integrated reports to monitor code coverage and provide insights into the quality and robustness of the test suite.

---

# Test Plan for MTOGO Project

## 1. Objectives
The test plan aims to implement a robust testing framework that ensures:
- Consistency across unit, integration, and system tests.
- The system’s scalability and flexibility, meeting both functional and non-functional requirements.
- Compliance with security, reliability, and performance standards.

## 2. Execution Plan

### CI/CD Integration
- **Automation:** Tests will run automatically via GitHub Actions on every code push or pull request.
- **Containerization:** Docker is used to ensure consistent testing environments across local and cloud-based systems.

### Code Coverage and Mutation Testing
- **Code Coverage:** Coverlet is used to track code coverage, ensuring reports are integrated into the CI/CD pipeline.
- **Mutation Testing:** Stryker.NET is used to introduce controlled mutations to the source code, helping to assess the robustness of tests and highlight any gaps in coverage.

## 3. Tooling and Automation

### **Testing Tools:**
- **xUnit:** Primary framework for unit and integration tests.
- **Moq:** For creating mock dependencies.
- **Stryker.NET:** For mutation testing.
- **Swagger/Postman:** For manual validation of APIs.
- **K6:** For load and performance testing.

### **CI/CD and Code Quality:**
- **GitHub Actions:** Used for automating test execution and ensuring continuous integration and delivery.
- **Docker:** Enables consistent test environments using containerization.
- **FxCop:** Used for static code analysis to detect potential issues and enforce adherence to coding standards.

## 4. Regression Testing
- Regression tests are integrated into the CI/CD pipeline to automatically validate that new changes do not introduce bugs or break existing features.
- Critical workflows, such as order placement and payment handling, are prioritized.

---

# **Technology Stack**

#### **Version Control Platform:**

* **Git:** Version control for collaborative development.  
* **GitHub:** Repository hosting.

#### **Development Environment:**

* **Visual Studio:** For coding and debugging.  
* **Swagger:** API documentation for testing and understanding endpoints.  
* **Redis Commander:** For managing the Redis database.  
* **SSMS:** For managing the SQL databases.


### **Development Stack**


#### **Backend Development:**


* **C\# and .NET Core:** For developing microservices.


#### **Database Management:**


* **Microsoft SQL Server:** Primary database for most services.  
* **Redis:** Temporary storage for shopping cart data.


#### **Message Queue:**


* **RabbitMQ:** Facilitates communication between microservices.


#### **Development Tools:**


* **Docker:** Containerization for consistent environments. used for rabbitmq and redis.  
* **Docker Compose:** For putting it all together.   
* **Ocelot:** Used for the API gateway.  
* **Dapper:** Used for data access and ORM purposes.


#### **CI/CD Pipeline:**


* **Dapper:** Used for automated tests and pushing docker images to Docker HUb


## **Ubiquitous Language** 

### **Entities:**

* **Customer:** The user placing an order.  
* **DeliveryAgent:** The individual delivering the food.  
* **Restaurant:** A business preparing food that can be ordered through the MTOGO platform.  
* **Menu:** A list of food items offered by a restaurant.  
* **Menu Item:** Individual dishes or offerings on the menu, available for customers to order.  
* **Order:** A food request made by a customer through the platform.  
* **Shopping Cart:** A temporary collection of items selected by a customer before placing an order.  
* **Review:** Feedback provided by customers for restaurants, including food ratings and comments.  
* **Address:** The location associated with restaurants or users.  
* **Food Category:** The type of cuisine or food offerings associated with a restaurant.


## **Architecture Documentation**

### **Project Structure:**

### **Microservices**

Each microservice is self-contained and responsible for its domain:

* **MTOGO.Services.AuthAPI**  
* **MTOGO.Services.ShoppingCartAPI**  
* **MTOGO.Services.OrderAPI**  
* **MTOGO.Services.EmailAPI**  
* **MTOGO.Services.RestaurantAPI**  
* **MTOGO.Services.ReviewAPI**

### **API Gateway**

The gateway solution is built with Ocelot, it creates an API gateway where we can access all of our microservice’s API endpoints through Swagger.

* **MTOGO.GatewaySolution**

### **Web Project**

Our web project implements the microservices through our API gateway, we created this to show the full flow of the system. 

* **MTOGO.Web**

### **Integration**

This project contains our RabbitMQ logic which we use in our microservices to publish and subscribe to messages/queues.

* **MTOGO.MessageBus**

---

## **Project Structure**

### **General Microservice Folder Structure:**

**`├── Properties`**

	├───────── launchSettings.json # Startup config

**`├── Controllers/`**    `# API controllers`

**`├── Models/`**         `# Data models and DTOs`

**`├── Dto/`**

**`├── Services/`**       `# Business logic services and interfaces`

	├───────── IServices/

**`├── Data/`**           `# Database context and migrations`

**`├── Program.cs`**      `# Application entry point`

**`├── Dockerfile`**      

**`└── appsettings.json`** `# Configuration settings`

---

## **Requirements and Diagrams**

**Domain Diagram:**

This diagram below illustrates the domain model for the MTOGO system, including a potential DeliveryAgent entity. Although not currently implemented, the DeliveryAgent entity represents a future enhancement where restaurants could outsource deliveries to dedicated agents.

![Domain Diagram](Documentation/Diagrams/DomainDiagram2.png)

**System Architecture Diagram:**

![System Architecture Diagram](Documentation/Diagrams/diagram3.png)

<details>
  <summary>EER Diagrams (Click to Expand)</summary>

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

</details>

