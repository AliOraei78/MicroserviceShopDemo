# MicroserviceShopDemo

A real-world Microservices project built with .NET 10, designed as a professional portfolio project for LinkedIn and GitHub.

## Architecture

* 6 Independent Services
* API Gateway (YARP)
* Message Broker (RabbitMQ + MassTransit)
* Docker Compose
* Health Checks
* Event-Driven Architecture

## Services

* ProductService
* CustomerService
* OrderService
* PaymentService
* NotificationService
* InventoryService

## Project Progress

### Day 1

* Created the solution structure and base projects

### Day 2

* Implemented ProductService with full CRUD operations
* Integrated Entity Framework Core
* Established the data access layer and database persistence

### Day 3

* Implemented CustomerService
* Applied the Repository Pattern for data access abstraction
* Improved maintainability, testability, and separation of concerns

### Day 4

* Implemented OrderService with domain-driven business logic
* Added order validation rules and business constraints
* Implemented order total calculation based on product data
* Integrated communication with ProductService for pricing information
* Separated business rules into a dedicated Domain Service



