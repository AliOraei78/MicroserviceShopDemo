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

* Implemented CustomerService using the Repository Pattern
* Improved maintainability, testability, and separation of concerns

### Day 4

* Implemented OrderService with Domain Logic
* Added order validation and business rules
* Integrated ProductService for order total calculation

### Day 5

* Implemented an API Gateway using YARP
* Established a single entry point for all microservices
* Configured request routing and service forwarding

### Day 6

* Implemented asynchronous communication using RabbitMQ and MassTransit
* Published domain events from OrderService
* Introduced Event-Driven Architecture principles
* Enabled loose coupling between microservices

### Day 7

* Implemented PaymentService and integrated it with the message broker
* Created consumers for processing order-related events
* Added asynchronous payment processing workflows
* Established the foundation for payment event publishing

### Day 8

* Implemented NotificationService
* Created event consumers for handling order notifications
* Integrated NotificationService with RabbitMQ and MassTransit
* Simulated Email notification delivery
* Simulated SMS notification delivery
* Enabled multiple services to react independently to the same domain event
* Strengthened the Event-Driven Architecture by adding cross-service notifications
