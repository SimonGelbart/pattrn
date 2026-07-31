# Introduction

This homework contains 3 sections (A, B, C) and each section contains numbered questions (A1, A2, A3 ...).

Answer each question by modifying the code or by writing your answer in the `Answers.md` document.

### Reminder:

- :warning: Remember to complete the `\docs\Answers.md` file with your answers  
  

# Routing exercise

The context of this exercise is a fictional messaging system.

Consumers can subscribe to messages of a given type.
When a message type is routable, the consumers can also subscribe to a subset of the messages, identified by a `ContentPattern`.

A `MessageRoutingContent` can be generated for every routable message. This object contains the routable components of the message and there can an unlimited number of routable components.

If a consumer subscribes using the pattern `{ "A", "*" }`, it will receive the messages `{ "A", "100" }` and `{ "A", "200" }`, but not `{ "B", "100" }`.

The `MessageRouter` is used to identity the consumers for a specified message.

The messaging system is expected to handle 1000+ consumers, 5000+ subscriptions per consumer and up to 2000+ subscriptions per consumer for a given message type.

## A. Implement SubscriptionIndex

- A1. Make `MessageRouterTests` tests pass
- A2. Add missing tests
- A3. Analyze and try to improve `MessageRouter.GetConsumers` performance 
    - Document your approach and the tools you used
    - Include screenshots of your analysis or your intermediate steps if appropriate

## B. Implement MessageQueue

- B1. Make `MessageQueueTests` tests pass
- B2. Make `MessageQueue` thread safe

## C. Improve SubscriptionIndex performances (Bonus)

- C1. Find a solution where the `MessageRouterBenchmarks.GetConsumers` benchmark executes in less that 10 microseconds (µs)
